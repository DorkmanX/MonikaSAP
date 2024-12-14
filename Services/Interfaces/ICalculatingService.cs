using MonikaSAP.Models;

namespace MonikaSAP.Services.Interfaces
{
    public interface ICalculatingService
    {
        public Response CalculateRawMaterialCost(string fileName);
    }
}
