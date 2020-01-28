using System;
using System.Collections;
using System.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using CenturyLink.ApplicationBlocks.ExceptionManager;
using CenturyLink.Network.Engineering.Material.Management.Business.DTO;
using CenturyLink.Network.Engineering.Material.Management.Manager;

namespace CenturyLink.Network.Engineering.Material.Management.Business.Utility
{
    internal class OSPFMSendMail
    {
        public ArrayList alSuccessMails = new ArrayList();
        public ArrayList alFailureMails = new ArrayList();
        long lBatchID = -1;

        static string FromEmailForSuccessfulParts;
        static string ToEmailForSuccessfulParts;
        static string SubjectEmailForSuccessfulParts;

        static string FromEmailForFailedParts;
        static string ToEmailForFailedParts;
        static string SubjectEmailForFailedParts;

        static string FromEmailForInfraLoadFailure;
        static string ToEmailForInfraLoadFailure;
        static string SubjectEmailForInfraLoadFailure;

        static string FromEmailForNoDataRecvd;
        static string ToEmailForNoDataRecvd;
        static string SubjectEmailForNoDataRecvd;

        static string FromEmailForSchemaValidationFailure;
        static string ToEmailForSchemaValidationFailure;
        static string SubjectEmailForSchemaValidationFailure;

        static string sSMTPServer;

        SmtpClient smtpClient;

        public OSPFMSendMail()
        {
            smtpClient = new SmtpClient(sSMTPServer);
        }

        static OSPFMSendMail()
        {
            sSMTPServer = Convert.ToString(ConfigurationManager.AppSettings["SMTPServer"]);

            FromEmailForSuccessfulParts = Convert.ToString(ConfigurationManager.AppSettings["FromEmailForSuccessfulParts"]);
            ToEmailForSuccessfulParts = Convert.ToString(ConfigurationManager.AppSettings["ToEmailForSuccessfulParts"]);
            SubjectEmailForSuccessfulParts = Convert.ToString(ConfigurationManager.AppSettings["SubjectEmailForSuccessfulParts"]);

            FromEmailForFailedParts = Convert.ToString(ConfigurationManager.AppSettings["FromEmailForFailedParts"]);
            ToEmailForFailedParts = Convert.ToString(ConfigurationManager.AppSettings["ToEmailForFailedParts"]);
            SubjectEmailForFailedParts = Convert.ToString(ConfigurationManager.AppSettings["SubjectEmailForFailedParts"]);

            FromEmailForInfraLoadFailure = Convert.ToString(ConfigurationManager.AppSettings["FromEmailForInfraLoadFailure"]);
            ToEmailForInfraLoadFailure = Convert.ToString(ConfigurationManager.AppSettings["ToEmailForInfraLoadFailure"]);
            SubjectEmailForInfraLoadFailure = Convert.ToString(ConfigurationManager.AppSettings["SubjectEmailForInfraLoadFailure"]);

            FromEmailForNoDataRecvd = Convert.ToString(ConfigurationManager.AppSettings["FromEmailForNoDataRecvd"]);
            ToEmailForNoDataRecvd = Convert.ToString(ConfigurationManager.AppSettings["ToEmailForNoDataRecvd"]);
            SubjectEmailForNoDataRecvd = Convert.ToString(ConfigurationManager.AppSettings["SubjectEmailForNoDataRecvd"]);

            FromEmailForSchemaValidationFailure = Convert.ToString(ConfigurationManager.AppSettings["FromEmailForSchemaValidationFailure"]);
            ToEmailForSchemaValidationFailure = Convert.ToString(ConfigurationManager.AppSettings["ToEmailForSchemaValidationFailure"]);
            SubjectEmailForSchemaValidationFailure = Convert.ToString(ConfigurationManager.AppSettings["SubjectEmailForSchemaValidationFailure"]);
        }

        #region Notification through Mail

        public StringBuilder BuildFailureMail(string sMtlCode, string sItmStatus, string sMtlDescr, long lStageID, ArrayList alErrors)
        {
            StringBuilder sFailureMail = new StringBuilder();
            //string sFailureMail = sProductID + Environment.NewLine + sItmCurrentStatus + Environment.NewLine + sItemDescr + Environment.NewLine;
            StringBuilder sErrMail = new StringBuilder();

            int i = 0;

            foreach (CatalogErrorDTO catMailDTO in alErrors)
            {
                if (i == 0)
                {
                    sFailureMail.Append("Material Code - " + sMtlCode + Environment.NewLine + "<br>");
                    sFailureMail.Append("Item Status - " + sItmStatus + Environment.NewLine + "<br>");
                    sFailureMail.Append("DESCR - " + sMtlDescr + Environment.NewLine + "<br>");
                }

                i++;

                if (catMailDTO.StageID == lStageID || catMailDTO.ErrorCode.Equals("E00001"))
                    sErrMail.Append(catMailDTO.ErrorMsg + Environment.NewLine + "<br>");
            }

            sFailureMail.Append(sErrMail);
            return sFailureMail;
        }

        public StringBuilder BuildFailureMail(string sMtlCode, string sItmStatus, string sMtlDescr, PackageOutputDTO pckOut)
        {

            StringBuilder sFailureMail = new StringBuilder();
            //string sFailureMail = sProductID + Environment.NewLine + sItmCurrentStatus + Environment.NewLine + sItemDescr + Environment.NewLine;

            sFailureMail.Append("Material Code - " + sMtlCode + Environment.NewLine + "<br>");
            sFailureMail.Append("Item Status - " + sItmStatus + Environment.NewLine + "<br>");
            sFailureMail.Append("DESCR - " + sMtlDescr + Environment.NewLine + "<br>");

            sFailureMail.Append(pckOut.MsgCode + ":" + pckOut.MsgText + "<br>");

            return sFailureMail;
        }

        public StringBuilder BuildSuccessMail(string sMtlCode, string sItmStatus, string sMtlDescr, int nNectasChgAttb)
        {
            StringBuilder sSuccessMail = new StringBuilder();
            //string sSuccessMail = sProductID + Environment.NewLine + sItmCurrentStatus + Environment.NewLine + sItemDescr + Environment.NewLine;

            sSuccessMail.Append("Material Code - " + sMtlCode + Environment.NewLine + "<br>");
            sSuccessMail.Append("Item Status - " + sItmStatus + Environment.NewLine + "<br>");
            sSuccessMail.Append("DESCR - " + sMtlDescr + Environment.NewLine + "<br>");

            sSuccessMail.Append("OSPFM Attribute Change - " + (nNectasChgAttb == -1 ? "N" : "Y") + "<br>");

            return sSuccessMail;
        }

        public StringBuilder BuildInfStructDataLoadFailureMail(long lBatchID, string sExcpMessage)
        {
            CatalogProcessingManager catMailMgr = new CatalogProcessingManager();
            StringBuilder sLoadFailMail = new StringBuilder();

            DateTime dtBatchTmStmp = catMailMgr.FetchBatchTimeStmp(lBatchID);

            sLoadFailMail.Append("Timestamp of Bus Service Message -" + dtBatchTmStmp.ToString() + Environment.NewLine + "<br>");
            sLoadFailMail.Append("Timestamp of Bus service failure -" + DateTime.Now.ToString() + Environment.NewLine + "<br>");
            sLoadFailMail.Append("Any other identification information - Batch ID" + lBatchID.ToString());
            sLoadFailMail.Append("List of failure messages - " + sExcpMessage + Environment.NewLine + "<br>");

            return sLoadFailMail;
        }

        /// <summary>
        /// This is used to send a notification mail with all the Successful parts
        /// Successful Data Validation And Load
        /// </summary>
        public void NotifySuccessfulTransactions(string sMailBody)
        {
            // string sSMTPServer = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
            //System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(System.Configuration.ConfigurationManager.AppSettings["FromEmailForSuccessfulParts"],
            //    System.Configuration.ConfigurationManager.AppSettings["ToEmailForSuccessfulParts"], System.Configuration.ConfigurationManager.AppSettings["SubjectEmailForSuccessfulParts"],
            //    sMailBody);
            MailMessage msg = new MailMessage(FromEmailForSuccessfulParts, ToEmailForSuccessfulParts, SubjectEmailForSuccessfulParts, sMailBody);
            msg.IsBodyHtml = true;

            //* System.Net.Mail.SmtpClient smtpClient = new SmtpClient(sSMTPServer);

            try
            {
                smtpClient.Send(msg);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Couldnt send email for Successful Parts/Transactions for BatchID. - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.NotifySuccessfulTransactions, sErrMsg, ExceptionConstants.Severity.Major);
            }

        }

        /// <summary>
        /// This is used to send a notification mail with all the Parts with Errors
        /// NECTAS Part Failures
        /// </summary>
        public void NotifyFailedTransactions(string sMailBody)
        {
            // string sSMTPServer = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
            //System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(System.Configuration.ConfigurationManager.AppSettings["FromEmailForSuccessfulParts"],
            //    System.Configuration.ConfigurationManager.AppSettings["ToEmailForSuccessfulParts"], System.Configuration.ConfigurationManager.AppSettings["SubjectEmailForSuccessfulParts"],
            //    sMailBody);
            MailMessage msg = new MailMessage(FromEmailForFailedParts, ToEmailForFailedParts, SubjectEmailForFailedParts, sMailBody);
            msg.IsBodyHtml = true;

            //* System.Net.Mail.SmtpClient smtpClient = new SmtpClient(sSMTPServer);

            try
            {
                smtpClient.Send(msg);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Couldnt send email for Failed Parts/Transactions for BatchID. - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.NotifyFailedTransactions, sErrMsg, ExceptionConstants.Severity.Major);
            }
        }

        /// <summary>
        /// This is used to send a notification mail for Infrastructure and Data Load Failure		
        /// </summary>
        public void NotifyInfrastructureDataLoadFailure(string sMailBody)
        {
            // string sSMTPServer = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
            //System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(System.Configuration.ConfigurationManager.AppSettings["FromEmailForSuccessfulParts"],
            //    System.Configuration.ConfigurationManager.AppSettings["ToEmailForSuccessfulParts"], System.Configuration.ConfigurationManager.AppSettings["SubjectEmailForSuccessfulParts"],
            //    sMailBody);
            MailMessage msg = new MailMessage(FromEmailForInfraLoadFailure, ToEmailForInfraLoadFailure, SubjectEmailForInfraLoadFailure, sMailBody);
            msg.IsBodyHtml = true;

            //* System.Net.Mail.SmtpClient smtpClient = new SmtpClient(sSMTPServer);

            try
            {
                smtpClient.Send(msg);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Couldnt send email for Infrastructure and Data Load Failure. - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.NotifyInfrastructureDataLoadFailure, sErrMsg, ExceptionConstants.Severity.Major);
            }
        }

        /// <summary>
        /// This is used to send a notification mail for "No Data Received For Schedule Runs"		
        /// </summary>
        public void NotifyNoCatalogDataReceived(string sMailBody)
        {
            // string sSMTPServer = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
            //System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(System.Configuration.ConfigurationManager.AppSettings["FromEmailForSuccessfulParts"],
            //    System.Configuration.ConfigurationManager.AppSettings["ToEmailForSuccessfulParts"], System.Configuration.ConfigurationManager.AppSettings["SubjectEmailForSuccessfulParts"],
            //    sMailBody);
            MailMessage msg = new MailMessage(FromEmailForNoDataRecvd, ToEmailForNoDataRecvd, SubjectEmailForNoDataRecvd, sMailBody);
            msg.IsBodyHtml = true;

            //* System.Net.Mail.SmtpClient smtpClient = new SmtpClient(sSMTPServer);

            try
            {
                smtpClient.Send(msg);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Couldnt send email for No Data Received For Schedule Runs - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.NotifyNoCatalogDataReceived, sErrMsg, ExceptionConstants.Severity.Major);
            }
        }

        /// <summary>
        /// This is used to send a notification mail for "Schema Validation Failure"		
        /// </summary>
        public void NotifySchemaValidationFailure(string sMailBody)
        {
            // string sSMTPServer = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
            //System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage(System.Configuration.ConfigurationManager.AppSettings["FromEmailForSuccessfulParts"],
            //    System.Configuration.ConfigurationManager.AppSettings["ToEmailForSuccessfulParts"], System.Configuration.ConfigurationManager.AppSettings["SubjectEmailForSuccessfulParts"],
            //    sMailBody);
            MailMessage msg = new MailMessage(FromEmailForSchemaValidationFailure, ToEmailForSchemaValidationFailure, SubjectEmailForSchemaValidationFailure, sMailBody);
            msg.IsBodyHtml = true;

            //* System.Net.Mail.SmtpClient smtpClient = new SmtpClient(sSMTPServer);

            try
            {
                smtpClient.Send(msg);
            }
            catch (Exception ex)
            {
                string sErrMsg = "Couldnt send email for Schema Validation Failure - " + ex.Message;
                //ExceptionManager.PublishException(ex, (int)Constants.SentryIdentifier.NotifyNoCatalogDataReceived, sErrMsg, ExceptionConstants.Severity.Major);
            }
        }


        #region  Sending Mail for Data Processing
        public void SendMail()
        {
            try
            {
                StringBuilder sSuccessfulParts = new StringBuilder();
                foreach (StringBuilder sMailBody in alSuccessMails)
                    sSuccessfulParts.Append(sMailBody + Environment.NewLine + Environment.NewLine + "<br>");

                if (sSuccessfulParts.ToString() != String.Empty)
                    NotifySuccessfulTransactions(sSuccessfulParts.ToString());

                StringBuilder sFailedParts = new StringBuilder();
                foreach (StringBuilder sMailBody in alFailureMails)
                    sFailedParts.Append(sMailBody + Environment.NewLine + Environment.NewLine + "<br>");

                if (sFailedParts.ToString() != String.Empty)
                    NotifyFailedTransactions(sFailedParts.ToString());
            }
            catch (Exception ex)
            {
                //Build Mail Body for Infrastructure failure.				
                NotifyInfrastructureDataLoadFailure(BuildInfStructDataLoadFailureMail(lBatchID, ex.Message).ToString());
            }
        }
        #endregion

        #endregion
    }
}
