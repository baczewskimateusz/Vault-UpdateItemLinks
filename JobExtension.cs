using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            long fileId = long.Parse(job.Params["FileId"]);
     
            Connection vaultConnection = context.Connection;

            WebServiceManager webServiceManager = vaultConnection.WebServiceManager;
            ItemService itemService = webServiceManager.ItemService;

            Item item = itemService.GetItemsByFileId(fileId).First();

            try
            {
                RefreshItemLinks(item, vaultConnection);
            }
            catch
            {
                throw;
            }
        }

        public void RefreshItemLinks(Item editItem, Connection vaultConn)
        {
            WebServiceManager webServiceManager = vaultConn.WebServiceManager;
            ItemService itemService = webServiceManager.ItemService;

            try
            {
                //    var linkTypeOptions = ItemFileLnkTypOpt.Primary
                //    | ItemFileLnkTypOpt.PrimarySub
                //    | ItemFileLnkTypOpt.Secondary
                //    | ItemFileLnkTypOpt.SecondarySub
                //    | ItemFileLnkTypOpt.StandardComponent
                //    | ItemFileLnkTypOpt.Tertiary;
                //    var assocs = itemService.GetItemFileAssociationsByItemIds(
                //        new long[] { editItem.Id }, linkTypeOptions);
                //    itemService.AddFilesToPromote(assocs.Select(x => x.CldFileId).ToArray(), ItemAssignAll.No, true);
                //    var promoteOrderResults = itemService.GetPromoteComponentOrder(out DateTime timeStamp);
                //    if (promoteOrderResults.PrimaryArray != null
                //        && promoteOrderResults.PrimaryArray.Any())
                //    {
                //        itemService.PromoteComponents(timeStamp, promoteOrderResults.PrimaryArray);
                //    }
                //    if (promoteOrderResults.NonPrimaryArray != null
                //        && promoteOrderResults.NonPrimaryArray.Any())
                //    {
                //        itemService.PromoteComponentLinks(promoteOrderResults.NonPrimaryArray);
                //    }
                //    var promoteResult = itemService.GetPromoteComponentsResults(timeStamp);

                //    Item[] items = promoteResult.ItemRevArray;


                //    itemService.UpdateAndCommitItems(items);

                itemService.UpdatePromoteComponents(new long[] { editItem.RevId } ,
                    ItemAssignAll.Default, false);

                DateTime timestamp;

                GetPromoteOrderResults promoteOrder =

                    itemService.GetPromoteComponentOrder(out timestamp);

                itemService.PromoteComponents(timestamp, promoteOrder.PrimaryArray);

                ItemsAndFiles itemsAndFiles =
                    itemService.GetPromoteComponentsResults(timestamp);

                List<Item> items = itemsAndFiles.ItemRevArray
                        .Where((x, index) => itemsAndFiles.StatusArray[index] == 4)
                        .ToList();

                itemService.UpdateAndCommitItems(items.ToArray());

            }
            catch
            {
                itemService.UndoEditItems(new long[] { editItem.Id });
                throw;
            }
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
