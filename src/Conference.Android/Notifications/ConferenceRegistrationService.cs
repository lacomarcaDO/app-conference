using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Iid;
using Conference.Clients.Portable;
using Android.Gms.Gcm;
using WindowsAzure.Messaging;

namespace Conference.Droid.Notifications
{
    [Service]
    public class ConferenceRegistrationService : IntentService
    {

        static NotificationHub hub;
        public static void Register(Context context)
        {
            try
            {
                if (ApiKeys.AzureServiceBusUrl == nameof(ApiKeys.AzureServiceBusUrl))
                    return;

                // Call this from our main activity
                var cs = ConnectionString.CreateUsingSharedAccessKeyWithListenAccess(
                    new Java.Net.URI(ApiKeys.AzureServiceBusUrl),
                    ApiKeys.AzureKey);

                hub = new NotificationHub(ApiKeys.AzureHubName, cs, context);
            }
            catch (Exception ex)
            {
                ex.Data["method"] = "GcmService.Initialize();";

            }

            context.StartService(new Intent(context, typeof(ConferenceRegistrationService)));
        }

        protected override void OnHandleIntent(Intent intent)
        {

            if (ApiKeys.AzureServiceBusUrl == nameof(ApiKeys.AzureServiceBusUrl))
                return;

            try
            {
                //Get the new token and send to the server
                var instanceID = InstanceID.GetInstance(Application.Context);
                var token = instanceID.GetToken(ApiKeys.GoogleSenderId, GoogleCloudMessaging.InstanceIdScope);


                var oldToken = Settings.Current.GcmToken;

                if (token != oldToken)
                    Settings.Current.GcmToken = token;

                Android.Util.Log.Debug("Evovle16", $"Gcm Token: {token}");           
               
                if (hub != null)
                    hub.Register(token);

                Settings.Current.PushRegistered = true;
            }
            catch (Exception ex)
            {
                ex.Data["method"] = "GcmService.OnRegistered();";

                Console.WriteLine("Unable to register Hub" + ex);
            }

        }
    }
}