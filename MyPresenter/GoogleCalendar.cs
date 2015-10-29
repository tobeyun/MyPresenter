using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace GoogleAPI
{
    class AccessToken
    {
        public string access_token;
        public string token_type;
        public string expires_in;
    }

    public class Acl
    {
        public Acl()
        {
            kind = String.Empty;
            etag = String.Empty;
            id = String.Empty;
            scope = new Scope();
            role = String.Empty;
        }

        public Acl(string _role, Scope _scope)
        {
            role = _role;
            scope = _scope;
        }

        [ScriptIgnore]
        public string kind, etag, id;

        public Scope scope;
        public string role;
    }

    public class Scope
    {
        public Scope()
        {
            type = String.Empty;
            value = String.Empty;
        }

        public Scope(string _type, string _value)
        {
            type = _type;
            value = _value;
        }

        public string type, value;
    }

    public class DefaultReminders
    {
        public DefaultReminders()
        {
            method = String.Empty;
            minutes = String.Empty;
        }

        public string method, minutes;
    }

    public class CreatorOrganizer
    {
        public CreatorOrganizer()
        {
            id = String.Empty;
            email = String.Empty;
            displayName = String.Empty;
            self = String.Empty;
        }

        public string id, email, displayName, self;
    }

    public class StartEnd
    {
        public StartEnd()
        {
            date = String.Empty;
            dateTime = String.Empty;
            timeZone = String.Empty;
        }

        public StartEnd(string _dateTime, string _timeZone)
        {
            date = String.Empty;

            dateTime = _dateTime;
            timeZone = _timeZone;
        }

        [ScriptIgnore]
        public string date;

        public string dateTime, timeZone;
    }

    public class Attendees
    {
        public Attendees()
        {
            id = String.Empty;
            email = String.Empty;
            displayName = String.Empty;
            organizer = String.Empty;
            self = String.Empty;
            resource = String.Empty;
            optional = String.Empty;
            responseStatus = String.Empty;
            comment = String.Empty;
            additionalGuests = String.Empty;
        }

        public Attendees(string _email, string _displayName)
        {
            id = String.Empty;
            organizer = String.Empty;
            self = String.Empty;
            resource = String.Empty;
            optional = String.Empty;
            responseStatus = String.Empty;
            comment = String.Empty;
            additionalGuests = String.Empty;

            email = _email;
            displayName = _displayName;
        }

        [ScriptIgnore]
        public string id, organizer, self, resource, optional, responseStatus, comment, additionalGuests;

        public string email, displayName;
    }

    public class ExtendedProperties
    {
        public ExtendedProperties()
        {
            privateKey = String.Empty;
            sharedKey = String.Empty;
        }

        public string privateKey, sharedKey;
    }

    public class Gadget
    {
        public Gadget()
        {
            type = String.Empty;
            title = String.Empty;
            link = String.Empty;
            iconLink = String.Empty;
            width = String.Empty;
            height = String.Empty;
            display = String.Empty;
            preferences = String.Empty;
        }

        public string type, title, link, iconLink, width, height, display, preferences;
    }

    public class Reminders
    {
        public Reminders()
        {
            useDefault = String.Empty;
            overrides = new DefaultReminders();
        }

        public string useDefault;
        public DefaultReminders overrides;
    }

    public class Source
    {
        public Source()
        {
            url = String.Empty;
            title = String.Empty;
        }

        public string url, title;
    }

    public class EventRequest
    {
        public EventRequest()
        {
            kind = String.Empty;
            status = String.Empty;
            summary = String.Empty;
            description = String.Empty;
            location = String.Empty;
            start = new StartEnd();
            end = new StartEnd();
            transparency = String.Empty;
            attendees = new Attendees[0];
        }

        public string kind;
        public string status;
        public string summary;
        public string description;
        public string location;
        public StartEnd start;
        public StartEnd end;
        public string transparency;
        public Attendees[] attendees;
    }

    public class Items
    {
        public Items()
        {
            kind = String.Empty;
            etag = String.Empty;
            id = String.Empty;
            status = String.Empty;
            htmlLink = String.Empty;
            created = String.Empty;
            updated = String.Empty;
            summary = String.Empty;
            description = String.Empty;
            location = String.Empty;
            colorId = String.Empty;
            creator = new CreatorOrganizer();
            organizer = new CreatorOrganizer();
            start = new StartEnd();
            end = new StartEnd();
            endTimeUnspecified = String.Empty;
            recurrence = String.Empty;
            recurringEventId = String.Empty;
            originalStartTime = new StartEnd();
            transparency = String.Empty;
            visibility = String.Empty;
            iCalUID = String.Empty;
            sequence = String.Empty;
            attendees = new Attendees[0];
            attendeesOmitted = String.Empty;
            extendedProperties = new ExtendedProperties();
            hangoutLink = String.Empty;
            gadget = new Gadget();
            anyoneCanAddSelf = String.Empty;
            guestsCanInviteOthers = String.Empty;
            guestsCanModify = String.Empty;
            guestsCanSeeOtherGuests = String.Empty;
            privateCopy = String.Empty;
            locked = String.Empty;
            reminders = new Reminders();
            source = new Source();
        }

        public string kind;
        public string etag;
        public string id;
        public string status;
        public string htmlLink;
        public string created;
        public string updated;
        public string summary;
        public string description;
        public string location;
        public string colorId;
        public CreatorOrganizer creator;
        public CreatorOrganizer organizer;
        public StartEnd start;
        public StartEnd end;
        public string endTimeUnspecified;
        public string recurrence;
        public string recurringEventId;
        public StartEnd originalStartTime;
        public string transparency;
        public string visibility;
        public string iCalUID;
        public string sequence;
        public Attendees[] attendees;
        public string attendeesOmitted;
        public ExtendedProperties extendedProperties;
        public string hangoutLink;
        public Gadget gadget;
        public string anyoneCanAddSelf;
        public string guestsCanInviteOthers;
        public string guestsCanModify;
        public string guestsCanSeeOtherGuests;
        public string privateCopy;
        public string locked;
        public Reminders reminders;
        public Source source;
    }

    public class EventList
    {
        public string kind;
        public string etag;
        public string summary;
        public string description;
        public string updated;
        public string timeZone;
        public string accessRole;
        public DefaultReminders[] reminders;
        public string nextPageToken;
        public string nextSyncToken;
        public Items[] items;
    }

    /// <summary>
    /// Summary description for GoogleCalendar
    /// </summary>
    public class GoogleCalendar
    {
        private string _email = String.Empty;
        private string _password = String.Empty;

        public GoogleCalendar()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private string getTimestamp(double add_minutes)
        {
            TimeSpan ts = (DateTime.Now.AddMinutes(add_minutes).ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0));

            return ts.TotalSeconds.ToString("0");
        }

        public void insertAcl(string role, string scope)
        {
            try
            {
                Acl acl = new Acl(role, new Scope(scope, "490770888960-u2qvsjj0lrgqsdfvk0iafip13t3dgbkm@developer.gserviceaccount.com"));

                JavaScriptSerializer js = new JavaScriptSerializer();

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/calendar/v3/calendars/primary/acl");

                req.Method = "POST";
                req.AllowAutoRedirect = false;
                req.ProtocolVersion = HttpVersion.Version11;
                req.Headers.Add("Authorization", "Bearer " + googleClientLogin());
                req.ContentType = "application/json";
                req.ContentLength = js.Serialize(acl).Length;

                using (StreamWriter streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII))
                {
                    streamOut.Write(js.Serialize(acl));
                }
            }
            catch (WebException ex)
            {
                //id = ex.Message;
            }
            finally { }
        }

        public string getCalendarEventsList(int start_day, int add_days)
        {
            string _id = String.Empty;
            string authToken = googleClientLogin();

            try
            {
                //insertAcl("reader", "default");

                string _request = "timeMax=" + HttpUtility.UrlEncode(DateTime.Now.AddDays(add_days).ToString("yyyy-MM-ddT00:00:00-04:00")) + "&" +
                                    "timeMin=" + HttpUtility.UrlEncode(DateTime.Now.AddDays(start_day).ToString("yyyy-MM-ddT00:00:00-04:00")) + "&" +
                                    "access_token=" + googleClientLogin();

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/calendar/v3/calendars/primary/events?" + _request);

                using (StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    _id = reader.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                _id = ex.Message;
            }
            finally { }

            return _id;
        }

        public string postCalendarEntry(string title, string who, string what, string where, string email, DateTime start_date_time, DateTime end_date_time)
        {
            string _id = String.Empty;
            string eventRequest = String.Empty;

            try
            {
                insertAcl("writer", "user");

                JavaScriptSerializer js = new JavaScriptSerializer();

                EventRequest item = new EventRequest();

                item.kind = "calendar#event";
                item.summary = title;
                item.description = what;
                item.transparency = "opaque";
                item.status = "confirmed";
                item.location = where;
                item.attendees = new Attendees[] { new Attendees("indybouncerentals@gmail.com", who) };
                item.start = new StartEnd(start_date_time.ToString("yyyy-MM-ddTHH:mm:sszzz"), "America/Indiana/Indianapolis");
                item.end = new StartEnd(end_date_time.AddHours(4).ToString("yyyy-MM-ddTHH:mm:sszzz"), "America/Indiana/Indianapolis");

                eventRequest = js.Serialize(item);

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/calendar/v3/calendars/primary/events");

                req.Method = "POST";
                req.AllowAutoRedirect = false;
                req.ProtocolVersion = HttpVersion.Version11;
                req.Headers.Add("Authorization", "Bearer " + googleClientLogin());
                req.ContentType = "application/json";
                req.ContentLength = eventRequest.Length;

                // write the request body to the stream
                using (StreamWriter streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII))
                {
                    streamOut.Write(eventRequest);
                }

                using (HttpWebResponse res = (HttpWebResponse)req.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                    {
                        Items _item = js.Deserialize<Items>(reader.ReadToEnd());

                        _id = _item.id;
                    }
                }
            }
            catch (WebException ex)
            {
                _id = ex.Message;
            }
            finally { }

            return _id;
        }

        private string googleClientLogin()
        {
            string authToken = String.Empty;

            try
            {
                string strRequest = "grant_type=urn%3Aietf%3Aparams%3Aoauth%3Agrant-type%3Ajwt-bearer&assertion=" + getSignatureBaseStringHash();

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://www.googleapis.com/oauth2/v3/token");

                req.ProtocolVersion = HttpVersion.Version11;
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = strRequest.Length;

                //Send the request to PayPal and get the response
                using (StreamWriter streamOut = new StreamWriter(req.GetRequestStream(), System.Text.Encoding.ASCII))
                {
                    streamOut.Write(strRequest);
                }

                using (StreamReader streamIn = new StreamReader(req.GetResponse().GetResponseStream()))
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();

                    AccessToken token = js.Deserialize<AccessToken>(streamIn.ReadToEnd());

                    authToken = token.access_token;
                }
            }
            catch (Exception ex)
            {
                //Response.Write("Auth Error: " + ex.Message + "<br /><br />");
            }
            finally { }

            return authToken;
        }

        private string getSignatureBaseStringHash()
        {
            string jwtHeader = HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes("{\"alg\":\"RS256\",\"typ\":\"JWT\"}"));
            string jwtClaim = HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes("{\"iss\":\"490770888960-u2qvsjj0lrgqsdfvk0iafip13t3dgbkm@developer.gserviceaccount.com\",\"scope\":\"https://www.googleapis.com/auth/calendar\",\"aud\":\"https://www.googleapis.com/oauth2/v3/token\",\"exp\":" + getTimestamp(60) + ",\"iat\":" + getTimestamp(0) + "}"));
            string jwtSignature = "";

            byte[] input = Encoding.UTF8.GetBytes(jwtHeader.Substring(0, jwtHeader.Length - 1) + "." + jwtClaim.Substring(0, jwtClaim.Length - 1));

            X509Certificate2 pkCert = new X509Certificate2("c:\\domains\\indybounce.com\\IndyBounce-bd19db4706bc.p12", "notasecret");

            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)pkCert.PrivateKey;
            CspParameters cspParam = new CspParameters
            {
                KeyContainerName = rsa.CspKeyContainerInfo.KeyContainerName,
                KeyNumber = rsa.CspKeyContainerInfo.KeyNumber == KeyNumber.Exchange ? 1 : 2
            };

            RSACryptoServiceProvider cryptoServiceProvider = new RSACryptoServiceProvider(cspParam) { PersistKeyInCsp = false };
            byte[] signatureBytes = cryptoServiceProvider.SignData(input, "SHA256");

            jwtSignature = HttpServerUtility.UrlTokenEncode(signatureBytes);

            return (jwtHeader.Substring(0, jwtHeader.Length - 1) + "." + jwtClaim.Substring(0, jwtClaim.Length - 1) + "." + jwtSignature.Substring(0, jwtSignature.Length - 1));
        }
    }
}
