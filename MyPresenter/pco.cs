using System;
using System.IO;
using System.Net;
using System.Text;

namespace MyPresenter
{
    public static class PCO
    {
        private static string live = "https://api.planningcenteronline.com/services/v2/service_types/384800/plans?filter=future";
        private static string sunday = "https://api.planningcenteronline.com/services/v2/service_types/308769/plans?filter=future";
        private static long ticks = 0;

        public static string pcoLastUpdate()
        {
            string ret = "";

            try
            {
                ret = getCurrentPlan().data.attributes.updated_at;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);

                //System.Windows.MessageBox.Show("Error loading PCO Plan. Check to make sure plan exists.", "PCO Import Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
            }
            
            return ret;
        }

        private static pcoPlan.Plan getCurrentPlan()
        {
            pcoPlan.Plan currentPlan = new pcoPlan.Plan();

            try
            {
                pcoPlans.Plans livePlans = new pcoPlans.Plans();
                pcoPlans.Plans sundayPlans = new pcoPlans.Plans();
                
                string currentPlanUrl;

                livePlans = JSonHelper.ConvertJSonToObject<pcoPlans.Plans>(getHttpWebResponse(live));
                sundayPlans = JSonHelper.ConvertJSonToObject<pcoPlans.Plans>(getHttpWebResponse(sunday));

                if (Convert.ToDateTime(livePlans.data[0].attributes.dates) < Convert.ToDateTime(sundayPlans.data[0].attributes.dates))
                    currentPlanUrl = livePlans.data[0].links.self;
                else
                    currentPlanUrl = sundayPlans.data[0].links.self;

                currentPlan = JSonHelper.ConvertJSonToObject<pcoPlan.Plan>(getHttpWebResponse(currentPlanUrl));
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }
            finally { }

            return currentPlan;
        }

        public static pcoItems.Items getItems()
        {
            pcoItems.Items items = new pcoItems.Items();

            try
            {
                items = JSonHelper.ConvertJSonToObject<pcoItems.Items>(getHttpWebResponse(getCurrentPlan().data.links.items));
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }
            finally { }

            return items;
        }

        private static string getHttpWebResponse(string url)
        {
            string response = String.Empty;

            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

                //Set values for the request back
                req.Method = "GET";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Headers.Add("Authorization", "Basic " + Base64Encode("643c3ee6d55c89a1c24468021627a43cd171579678da0d70baf87509ac99aad1:addab15c618c6dc42309325faefc8b42898f7a4dd2d77fbeb9bf1d8735f9c3cc"));

                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    // Pipes the stream to a higher level stream reader with the required encoding format. 
                    using (StreamReader readStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8))
                    {
                        response = readStream.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", ex.Message);
            }
            finally { }

            return response;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
