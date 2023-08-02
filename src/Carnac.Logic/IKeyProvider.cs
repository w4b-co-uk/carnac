using Carnac.Logic.Models;
using System;

namespace Carnac.Logic {
    public interface IKeyProvider {
        IObservable<KeyPress> GetKeyStream();
    }
}