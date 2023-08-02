using Carnac.Logic.Models;
using SettingsProviderNet;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Carnac.Logic {
    public class KeysController: IDisposable {
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        private readonly TimeSpan fadeOutDelay;
        private readonly ObservableCollection<Message> messages;
        private readonly IMessageProvider messageProvider;
        private readonly IConcurrencyService concurrencyService;
        private readonly SingleAssignmentDisposable actionSubscription = new SingleAssignmentDisposable();

        public KeysController(ObservableCollection<Message> messages, IMessageProvider messageProvider, IConcurrencyService concurrencyService, ISettingsProvider settingsProvider) {
            this.messages = messages;
            this.messageProvider = messageProvider;
            this.concurrencyService = concurrencyService;

            PopupSettings settings = settingsProvider.GetSettings<PopupSettings>();
            fadeOutDelay = TimeSpan.FromSeconds(settings.ItemFadeDelay);
        }

        public void Start() {
            System.Reactive.Subjects.IConnectableObservable<Message> messageStream = messageProvider.GetMessageStream().Publish();

            IDisposable addMessageSubscription = messageStream
                .ObserveOn(concurrencyService.MainThreadScheduler)
                .Subscribe(newMessage => {
                    if (newMessage.Previous != null) {
                        _ = messages.Remove(newMessage.Previous);
                    }
                    messages.Add(newMessage);
                });

            System.Reactive.Subjects.IConnectableObservable<Message> fadeOutMessageSeq = messageStream
                .Delay(fadeOutDelay, concurrencyService.Default)
                .Select(m => m.FadeOut())
                .Publish();

            IDisposable fadeOutMessageSubscription = fadeOutMessageSeq
                .ObserveOn(concurrencyService.MainThreadScheduler)
                .Subscribe(msg => {
                    int idx = messages.IndexOf(msg.Previous);
                    if (idx > -1) {
                        messages[idx] = msg;
                    }
                });

            // Finally we just put a one second delay on the messages from the fade out stream and flag to remove.
            IDisposable removeMessageSubscription = fadeOutMessageSeq
                .Delay(OneSecond, concurrencyService.Default)
                .ObserveOn(concurrencyService.MainThreadScheduler)
                .Subscribe(msg => messages.Remove(msg));

            actionSubscription.Disposable = new CompositeDisposable(
                addMessageSubscription,
                fadeOutMessageSubscription,
                removeMessageSubscription,
                fadeOutMessageSeq.Connect(),
                messageStream.Connect());
        }

        public void Dispose() {
            actionSubscription.Dispose();
        }
    }
}