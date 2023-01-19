using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RadarGame.UI.Models;

namespace RadarGame.UI.Interfaces
{
    public interface ISettingsService
    {
        UserSettings Current { get; set; }
        Task LoadAsync();
        Task SaveAsync();
    }
}
