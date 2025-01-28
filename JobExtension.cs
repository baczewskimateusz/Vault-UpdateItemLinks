using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Connectivity.Extensibility.Framework;
using Autodesk.Connectivity.JobProcessor.Extensibility;
using Autodesk.Connectivity.WebServices;
using Autodesk.Connectivity.WebServicesTools;
using Autodesk.DataManagement.Client.Framework.Vault.Currency.Connections;
using Autodesk.DataManagement.Client.Framework.Vault.Currency.Entities;

[assembly: ApiVersion("15.0")]
[assembly: ExtensionId("64af5dc7-92ec-4f3f-8e41-756510f6b3ae")]


namespace UpdateItemLinks
{
    public class JobExtension : IJobHandler
    {
        private static string JOB_TYPE = "KRATKI.UpdateItemLinks";

        #region IJobHandler Implementation
        public bool CanProcess(string jobType)
        {
            return jobType == JOB_TYPE;
        }

        public JobOutcome Execute(IJobProcessorServices context, IJob job)
        {
            try
            {
                UpdateItemLinks(context, job);
                return JobOutcome.Success;
            }
            catch (Exception ex)
            {
                context.Log(ex, "Job-Template Job failed: " + ex.ToString() + " ");
                return JobOutcome.Failure;
            }

        }

        private void UpdateItemLinks(IJobProcessorServices context, IJob job)
        {
            int fileId = int.Parse(job.Params["FileId"]);
            Connection vaultConnection = context.Connection;

            Item item =vaultConnection.WebServiceManager.ItemService.GetItemsByFileId(fileId).First();

            Refresh_ItemLinks(item, vaultConnection);
        }

        public void Refresh_ItemLinks(Item editItem, Connection vaultConn)
        {
            var linkTypeOptions = ItemFileLnkTypOpt.Primary
                | ItemFileLnkTypOpt.PrimarySub
                | ItemFileLnkTypOpt.Secondary
                | ItemFileLnkTypOpt.SecondarySub
                | ItemFileLnkTypOpt.StandardComponent
                | ItemFileLnkTypOpt.Tertiary;
            var assocs = vaultConn.WebServiceManager.ItemService.GetItemFileAssociationsByItemIds(
                new long[] { editItem.Id }, linkTypeOptions);
            vaultConn.WebServiceManager.ItemService.AddFilesToPromote(
                assocs.Select(x => x.CldFileId).ToArray(), ItemAssignAll.No, true);
            var promoteOrderResults = vaultConn.WebServiceManager.ItemService.GetPromoteComponentOrder(out DateTime timeStamp);
            if (promoteOrderResults.PrimaryArray != null
                && promoteOrderResults.PrimaryArray.Any())
            {
                vaultConn.WebServiceManager.ItemService.PromoteComponents(timeStamp, promoteOrderResults.PrimaryArray);
            }
            if (promoteOrderResults.NonPrimaryArray != null
                && promoteOrderResults.NonPrimaryArray.Any())
            {
                vaultConn.WebServiceManager.ItemService.PromoteComponentLinks(promoteOrderResults.NonPrimaryArray);
            }
            var promoteResult = vaultConn.WebServiceManager.ItemService.GetPromoteComponentsResults(timeStamp);
        }

        public void OnJobProcessorShutdown(IJobProcessorServices context)
        {
            //throw new NotImplementedException();
        }
        public void OnJobProcessorSleep(IJobProcessorServices context)
        {
            //throw new NotImplementedException();
        }
        public void OnJobProcessorStartup(IJobProcessorServices context)
        {
            //throw new NotImplementedException();
        }
        public void OnJobProcessorWake(IJobProcessorServices context)
        {
            //throw new NotImplementedException();
        }
        #endregion IJobHandler Implementation
    }
}
