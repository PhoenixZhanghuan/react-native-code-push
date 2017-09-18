﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using Windows.Storage;

namespace CodePush.ReactNative
{
    internal class SettingsManager
    {
        private static ApplicationDataContainer Settings = ApplicationData.Current.LocalSettings.CreateContainer(CodePushConstants.CodePushPreferences, ApplicationDataCreateDisposition.Always);

        public static JArray GetFailedUpdates()
        {
            var failedUpdatesString = (string)Settings.Values[CodePushConstants.FailedUpdatesKey];
            if (failedUpdatesString == null)
            {
                return new JArray();
            }

            try
            {
                return JArray.Parse(failedUpdatesString);
            }
            catch (Exception)
            {
                var emptyArray = new JArray();
                Settings.Values[CodePushConstants.FailedUpdatesKey] = JsonConvert.SerializeObject(emptyArray);
                return emptyArray;
            }
        }

        internal static JObject GetPendingUpdate()
        {
            var pendingUpdateString = (string)Settings.Values[CodePushConstants.PendingUpdateKey];
            if (pendingUpdateString == null)
            {
                return null;
            }

            try
            {
                return JObject.Parse(pendingUpdateString);
            }
            catch (Exception)
            {
                // Should not happen.
                CodePushUtils.Log("Unable to parse pending update metadata " + pendingUpdateString +
                        " stored in SharedPreferences");
                return null;
            }
        }

        internal static bool IsFailedHash(string packageHash)
        {
            JArray failedUpdates = SettingsManager.GetFailedUpdates();
            if (packageHash != null)
            {
                foreach (var failedPackage in failedUpdates)
                {
                    var failedPackageHash = (string)failedPackage[CodePushConstants.PackageHashKey];
                    if (packageHash.Equals(failedPackageHash))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool IsPendingUpdate(string packageHash)
        {
            JObject pendingUpdate = SettingsManager.GetPendingUpdate();
            return pendingUpdate != null &&
                    !(bool)pendingUpdate[CodePushConstants.PendingUpdateIsLoadingKey] &&
                    (packageHash == null || ((string)pendingUpdate[CodePushConstants.PendingUpdateHashKey]).Equals(packageHash));
        }

        internal static void RemoveFailedUpdates()
        {
            Settings.Values.Remove(CodePushConstants.FailedUpdatesKey);
        }

        internal static void RemovePendingUpdate()
        {
            Settings.Values.Remove(CodePushConstants.PendingUpdateKey);
        }

        internal static void SaveFailedUpdate(JObject failedPackage)
        {
            var failedUpdatesString = (string)Settings.Values[CodePushConstants.FailedUpdatesKey];
            JArray failedUpdates;
            if (failedUpdatesString == null)
            {
                failedUpdates = new JArray();
            }
            else
            {
                failedUpdates = JArray.Parse(failedUpdatesString);
            }

            failedUpdates.Add(failedPackage);
            Settings.Values[CodePushConstants.FailedUpdatesKey] = JsonConvert.SerializeObject(failedUpdates);
        }

        internal static void SavePendingUpdate(string packageHash, bool isLoading)
        {
            var pendingUpdate = new JObject()
            {
                { CodePushConstants.PendingUpdateHashKey, packageHash },
                { CodePushConstants.PendingUpdateIsLoadingKey, isLoading }
            };

            Settings.Values[CodePushConstants.PendingUpdateKey] = JsonConvert.SerializeObject(pendingUpdate);
        }
    }
}