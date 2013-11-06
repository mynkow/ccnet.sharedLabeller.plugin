using System.IO;
using System.Text;
using Exortech.NetReflector;
using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Remote;

namespace ThoughtWorks.CruiseControl.Core.Label
{
    [ReflectorType("sharedLabeller")]   // this is the labeller name that will be used in  ccnet.config
    public class SharedLabeller : LabellerBase
    {
        [ReflectorProperty("syncronisationFilePath", Required = true)]
        public string SyncronisationFilePath { get; set; }

        public override string Generate(IIntegrationResult integrationResult)
        {
            if (ShouldIncrementLabel(integrationResult))
                return IncrementLabel();
            if (integrationResult.Status == IntegrationStatus.Unknown)
                return "0"; return integrationResult.Label;
        }

        private string IncrementLabel()
        {
            if (!File.Exists(SyncronisationFilePath))
                return "0";
            using (FileStream fileStream = File.Open(SyncronisationFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                // read last build number from file
                var bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
                string rawBuildNumber = Encoding.ASCII.GetString(bytes);
                // parse last build number           
                int previousBuildNumber = int.Parse(rawBuildNumber);
                int newBuildNumber = previousBuildNumber + 1;
                // increment build number and write back to file
                bytes = Encoding.ASCII.GetBytes(newBuildNumber.ToString());
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Write(bytes, 0, bytes.Length);
                return newBuildNumber.ToString();
            }
        }

        private static bool ShouldIncrementLabel(IIntegrationResult previousResult)
        {
            return (previousResult.Status == IntegrationStatus.Success || previousResult.Status == IntegrationStatus.Unknown);
        }
    }
}