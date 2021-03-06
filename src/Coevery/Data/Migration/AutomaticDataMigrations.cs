﻿using System;
using System.Linq;
using Coevery.Environment;
using Coevery.Environment.Features;
using Coevery.Logging;

namespace Coevery.Data.Migration {
    /// <summary>
    /// Registers to CoeveryShell.Activated in order to run migrations automatically 
    /// </summary>
    public class AutomaticDataMigrations : ICoeveryShellEvents {
        private readonly IDataMigrationManager _dataMigrationManager;
        private readonly IFeatureManager _featureManager;

        public AutomaticDataMigrations(
            IDataMigrationManager dataMigrationManager,
            IFeatureManager featureManager
            ) {
            _dataMigrationManager = dataMigrationManager;
            _featureManager = featureManager;

            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; } 

        public void Activated() {

            // Let's make sure that the basic set of features is enabled.  If there are any that are not enabled, then let's enable them first.
            var theseFeaturesShouldAlwaysBeActive = new[] {
                "Settings"
            };

            var enabledFeatures = _featureManager.GetEnabledFeatures().Select(f => f.Id).ToList();
            var featuresToEnable = theseFeaturesShouldAlwaysBeActive.Where(shouldBeActive => !enabledFeatures.Contains(shouldBeActive)).ToList();
            if (featuresToEnable.Any()) {
                _featureManager.EnableFeatures(featuresToEnable, true);
            }

            foreach (var feature in _dataMigrationManager.GetFeaturesThatNeedUpdate()) {
                try {
                    _dataMigrationManager.Update(feature);
                }
                catch (Exception e) {
                    Logger.Error("Could not run migrations automatically on " + feature, e);
                }
            }
        }

        public void Terminating() {
            
        }
    }
}
