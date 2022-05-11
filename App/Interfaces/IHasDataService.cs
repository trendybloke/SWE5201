using App.Services;

namespace App.Interfaces
{
    internal interface IHasDataService
    {
        DataService dataService { get; }
    }
}
