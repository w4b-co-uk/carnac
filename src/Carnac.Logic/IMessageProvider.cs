using Carnac.Logic.Models;
using System;

namespace Carnac.Logic {
    public interface IMessageProvider {
        IObservable<Message> GetMessageStream();
    }
}