using Carnac.Logic;
using Carnac.Logic.KeyMonitor;
using Carnac.Logic.Models;
using Microsoft.Win32;
using NSubstitute;
using SettingsProviderNet;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Carnac.Tests {
    public class KeyProviderTests {
        private readonly IPasswordModeService passwordModeService;
        private readonly IDesktopLockEventService desktopLockEventService;
        private readonly ISettingsProvider settingsProvider;

        public KeyProviderTests() {
            passwordModeService = new PasswordModeService();
            desktopLockEventService = Substitute.For<IDesktopLockEventService>();
            _ = desktopLockEventService.GetSessionSwitchStream().Returns(Observable.Never<SessionSwitchEventArgs>());
            settingsProvider = Substitute.For<ISettingsProvider>();
        }

        [Fact]
        public async Task ctrlshiftl_is_processed_correctly() {
            // arrange
            KeyPlayer player = KeyStreams.CtrlShiftL();
            KeyProvider provider = new KeyProvider(player, passwordModeService, desktopLockEventService, settingsProvider);

            // act
            System.Collections.Generic.IList<KeyPress> processedKeys = await provider.GetKeyStream().ToList();

            // assert
            Assert.Equal(new[] { "Ctrl", "Shift", "L" }, processedKeys.Single().Input);
        }

        [Fact]
        public async Task shift_is_not_outputted_when_is_being_used_as_a_modifier_key() {
            // arrange
            KeyPlayer player = KeyStreams.ShiftL();
            KeyProvider provider = new KeyProvider(player, passwordModeService, desktopLockEventService, settingsProvider);

            // act
            System.Collections.Generic.IList<KeyPress> processedKeys = await provider.GetKeyStream().ToList();

            // assert

            Assert.Equal(new[] { "L" }, processedKeys.Single().Input);
        }

        [Fact]
        public async Task key_without_shift_is_lowercase() {
            // arrange
            KeyPlayer player = KeyStreams.LetterL();
            KeyProvider provider = new KeyProvider(player, passwordModeService, desktopLockEventService, settingsProvider);

            // act
            System.Collections.Generic.IList<KeyPress> processedKeys = await provider.GetKeyStream().ToList();

            // assert
            Assert.Equal(new[] { "l" }, processedKeys.Single().Input);
        }

        [Fact]
        public async Task verify_number() {
            // arrange
            KeyPlayer player = KeyStreams.Number1();
            KeyProvider provider = new KeyProvider(player, passwordModeService, desktopLockEventService, settingsProvider);

            // act
            System.Collections.Generic.IList<KeyPress> processedKeys = await provider.GetKeyStream().ToList();

            // assert
            Assert.Equal(new[] { "1" }, processedKeys.Single().Input);
        }

        [Fact]
        public async Task verify_shift_number() {
            // arrange
            KeyPlayer player = KeyStreams.ExclaimationMark();
            KeyProvider provider = new KeyProvider(player, passwordModeService, desktopLockEventService, settingsProvider);

            // act
            System.Collections.Generic.IList<KeyPress> processedKeys = await provider.GetKeyStream().ToList();

            // assert
            Assert.Equal(new[] { "!" }, processedKeys.Single().Input);
        }

        [Fact]
        public async Task keyprovider_detects_windows_key_presses() {
            // arrange
            KeyPlayer player = KeyStreams.WinkeyE();
            KeyProvider provider = new KeyProvider(player, passwordModeService, desktopLockEventService, settingsProvider);

            // act
            System.Collections.Generic.IList<KeyPress> processedKeys = await provider.GetKeyStream().ToList();

            // assert
            Assert.Equal(new[] { "Win", "e" }, processedKeys.Single().Input);
        }

        [Fact]
        public async Task output_with_matching_filter() {
            // arrange
            string currentProcessName = AssociatedProcessUtilities.GetAssociatedProcess().ProcessName;
            _ = settingsProvider.GetSettings<PopupSettings>().Returns(new PopupSettings() { ProcessFilterExpression = currentProcessName });
            KeyPlayer player = KeyStreams.LetterL();
            KeyProvider provider = new KeyProvider(player, passwordModeService, desktopLockEventService, settingsProvider);

            // act
            System.Collections.Generic.IList<KeyPress> processedKeys = await provider.GetKeyStream().ToList();

            // assert
            Assert.Equal(new[] { "l" }, processedKeys.Single().Input);
        }

        [Fact]
        public async Task no_output_with_no_match_filter() {
            // arrange
            _ = settingsProvider.GetSettings<PopupSettings>().Returns(new PopupSettings() { ProcessFilterExpression = "notepad" });
            KeyPlayer player = KeyStreams.LetterL();
            KeyProvider provider = new KeyProvider(player, passwordModeService, desktopLockEventService, settingsProvider);

            // act
            System.Collections.Generic.IList<KeyPress> processedKeys = await provider.GetKeyStream().ToList();

            // assert
            Assert.Equal(0, processedKeys.Count);
        }
    }
}