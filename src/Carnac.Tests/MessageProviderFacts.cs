using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Microsoft.Win32;
using NSubstitute;
using SettingsProviderNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

namespace Carnac.Tests {
    public class MessageProviderFacts {
        private readonly IShortcutProvider shortcutProvider;

        public MessageProviderFacts() {
            shortcutProvider = Substitute.For<IShortcutProvider>();
            _ = shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>()).Returns(new List<KeyShortcut>());

        }

        private MessageProvider CreateMessageProvider(IObservable<InterceptKeyEventArgs> keysStreamSource) {
            IInterceptKeys source = Substitute.For<IInterceptKeys>();
            _ = source.GetKeyStream().Returns(keysStreamSource);
            IDesktopLockEventService desktopLockEventService = Substitute.For<IDesktopLockEventService>();
            ISettingsProvider settingsProvider = Substitute.For<ISettingsProvider>();
            _ = desktopLockEventService.GetSessionSwitchStream().Returns(Observable.Never<SessionSwitchEventArgs>());
            KeyProvider keyProvider = new KeyProvider(source, new PasswordModeService(), desktopLockEventService, settingsProvider);
            return new MessageProvider(shortcutProvider, keyProvider, new PopupSettings());
        }

        [Fact]
        public async Task key_with_modifiers_raises_a_new_message() {
            // arrange
            IObservable<InterceptKeyEventArgs> keySequence = KeyStreams.LetterL()
                .Concat(KeyStreams.CtrlShiftL())
                .ToObservable();
            MessageProvider sut = CreateMessageProvider(keySequence);

            // act
            IList<Logic.Models.Message> messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(2, messages.Count);
        }

        [Fact]
        public async Task recognises_shortcuts() {
            // arrange
            IObservable<InterceptKeyEventArgs> keySequence = KeyStreams.CtrlShiftL().ToObservable();
            MessageProvider sut = CreateMessageProvider(keySequence);
            _ = shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("MyShortcut", new KeyPressDefinition(Keys.L, shiftPressed: true, controlPressed: true)) });

            // act
            IList<Logic.Models.Message> messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(1, messages.Count);
            Assert.Equal("MyShortcut", messages[0].ShortcutName);
        }

        [Fact]
        public async Task does_not_show_key_press_on_partial_match() {
            // arrange
            IObservable<InterceptKeyEventArgs> keySequence = KeyStreams.CtrlU().ToObservable();
            MessageProvider sut = CreateMessageProvider(keySequence);
            _ = shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            IList<Logic.Models.Message> messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(0, messages.Count);
        }

        [Fact]
        public async Task produces_two_messages_when_shortcut_is_broken() {
            // arrange
            IObservable<InterceptKeyEventArgs> keySequence = KeyStreams.CtrlU()
                .Concat(KeyStreams.Number1())
                .ToObservable();
            MessageProvider sut = CreateMessageProvider(keySequence);
            _ = shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            IList<Logic.Models.Message> messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(2, messages.Count);
            Assert.Equal("Ctrl + U", string.Join("", messages[0].Text));
            Assert.Equal("1", string.Join("", messages[1].Text));
        }

        [Fact]
        public async Task does_show_shortcut_name_on_full_match() {
            // arrange
            IObservable<InterceptKeyEventArgs> keySequence = KeyStreams.CtrlU()
                .Concat(KeyStreams.LetterL())
                .ToObservable();
            MessageProvider sut = CreateMessageProvider(keySequence);
            _ = shortcutProvider.GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            IList<Logic.Models.Message> messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(1, messages.Count);
            Assert.Equal("SomeShortcut", messages[0].ShortcutName);
        }

        [Fact]
        public async Task keeps_order_of_streams() {
            // arrange
            IObservable<InterceptKeyEventArgs> keySequence = KeyStreams.CtrlU()
                .Concat(KeyStreams.LetterL())
                .Concat(KeyStreams.Number1())
                .Concat(KeyStreams.LetterL())
                .ToObservable();
            MessageProvider sut = CreateMessageProvider(keySequence);
            _ = shortcutProvider
                .GetShortcutsStartingWith(Arg.Any<KeyPress>())
                .Returns(new List<KeyShortcut> { new KeyShortcut("SomeShortcut",
                    new KeyPressDefinition(Keys.U, controlPressed: true),
                    new KeyPressDefinition(Keys.L)) });

            // act
            IList<Logic.Models.Message> messages = await sut.GetMessageStream().ToList();

            // assert
            Assert.Equal(3, messages.Count);
            Assert.Equal("Ctrl + U, l [SomeShortcut]", string.Join("", messages[0].Text));
            Assert.Equal("SomeShortcut", messages[0].ShortcutName);
            Assert.Equal("1", string.Join("", messages[1].Text));
            Assert.Equal("1l", string.Join("", messages[2].Text));
        }
    }
}