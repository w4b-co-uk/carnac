using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Microsoft.Reactive.Testing;
using NSubstitute;
using SettingsProviderNet;
using Shouldly;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Xunit;
using Message = Carnac.Logic.Models.Message;

namespace Carnac.Tests {
    public class KeysControllerFacts {
        private const int MessageAOnNextTick = 100;
        private readonly ObservableCollection<Message> messages = new ObservableCollection<Message>();
        private readonly TestScheduler testScheduler;
        private readonly Message messageA = new Message(A);

        public KeysControllerFacts() {
            testScheduler = new TestScheduler();
        }

        private KeysController CreateKeysController(IObservable<Message> messageStream) {
            IMessageProvider messageProvider = Substitute.For<IMessageProvider>();
            _ = messageProvider.GetMessageStream().Returns(_ => messageStream);
            IConcurrencyService concurrencyService = Substitute.For<IConcurrencyService>();
            _ = concurrencyService.MainThreadScheduler.Returns(testScheduler);
            _ = concurrencyService.Default.Returns(testScheduler);

            ISettingsProvider settingsService = Substitute.For<ISettingsProvider>();
            PopupSettings popupSettings = new PopupSettings();
            popupSettings.ItemFadeDelay = GetDefaultFadeDelay(popupSettings);
            _ = settingsService.GetSettings<PopupSettings>().Returns(popupSettings);

            return new KeysController(messages, messageProvider, concurrencyService, settingsService);
        }

        [Fact]
        public void MessagesAreAddedIntoKeysColletion() {
            KeysController sut = CreateKeysController(SingleMessageAt100Ticks());
            sut.Start();
            testScheduler.AdvanceTo(MessageAOnNextTick + 1);  //+1 for the cost of the ObserveOn scheduling

            messages.ShouldContain(messageA);
            messageA.IsDeleting.ShouldBe(false);
        }

        [Fact]
        public void MessagesAreFlaggedAsDeletingAfter5Seconds() {
            KeysController sut = CreateKeysController(SingleMessageAt100Ticks());
            sut.Start();
            testScheduler.AdvanceBy(MessageAOnNextTick + 1);
            testScheduler.AdvanceBy(5.Seconds());

            _ = Assert.Single(messages);
            messages.Single().ShouldBe(messageA.FadeOut());
        }

        [Fact]
        public void MessagesIsRemovedAfter6Seconds() {
            KeysController sut = CreateKeysController(SingleMessageAt100Ticks());
            sut.Start();
            testScheduler.AdvanceBy(MessageAOnNextTick + 2);
            testScheduler.AdvanceBy(6.Seconds());

            messages.ShouldBeEmpty();
        }

        [Fact]
        public void MessageTimeoutIsStartedAgainIfMessageIsUpdated() {
            Message expected = messageA.Merge(new Message(A));
            ITestableObservable<Message> messageSequence = testScheduler.CreateColdObservable(
                ReactiveTest.OnNext(MessageAOnNextTick, messageA),
                ReactiveTest.OnNext(3.Seconds(), expected)
                );

            KeysController sut = CreateKeysController(messageSequence);

            sut.Start();
            testScheduler.AdvanceBy(6.Seconds());

            messages.Single().IsDeleting.ShouldBe(false);
            messages.Single().ShouldBe(expected);
        }

        [Fact]
        public void MultiMerge() {
            Message message1 = new Message(Down);
            Message message2 = message1.Merge(new Message(Down));
            Message message3 = message2.Merge(new Message(Down));

            Message expected = message3;
            ITestableObservable<Message> messageSequence = testScheduler.CreateColdObservable(
                ReactiveTest.OnNext(0.1.Seconds(), message1),
                ReactiveTest.OnNext(0.2.Seconds(), message2),
                ReactiveTest.OnNext(0.3.Seconds(), message3)
                );

            KeysController sut = CreateKeysController(messageSequence);

            sut.Start();
            testScheduler.AdvanceBy(1.Seconds());

            messages.Single().IsDeleting.ShouldBe(false);
            messages.Single().ShouldBe(expected);
        }

        private static KeyPress A => new KeyPress(new ProcessInfo("foo"),
                    new InterceptKeyEventArgs(Keys.A, KeyDirection.Down, false, false, false), false, new[] { "a" });

        private static KeyPress Down => new KeyPress(new ProcessInfo("foo"),
                    new InterceptKeyEventArgs(Keys.Down, KeyDirection.Down, false, false, false), false, new[] { "Down" });

        private IObservable<Message> SingleMessageAt100Ticks() {
            return testScheduler.CreateColdObservable(
               ReactiveTest.OnNext(MessageAOnNextTick, messageA)
               );
        }

        private double GetDefaultFadeDelay(PopupSettings settings) {
            AttributeCollection attributes =
                TypeDescriptor.GetProperties(settings)["ItemFadeDelay"].Attributes;
            DefaultValueAttribute myAttribute =
                (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];

            _ = double.TryParse(myAttribute.Value.ToString(), out double fadeDelay);
            return fadeDelay;
        }
    }
}
