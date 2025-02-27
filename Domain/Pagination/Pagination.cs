using Newtonsoft.Json;

namespace API.Domain.Pagination
{
    public class Pagination
    {
              
        [JsonProperty("pageNumber")]
        public int? PageNumber { get; set; } = 1;

        [JsonProperty("pageSize")]
        public int? PageSize { get; set; } = 10;
    
        public static void ConfigPagination(PaginationRequest request)
        {
            request.PageNumber ??= 1;
            request.PageNumber ??= 10;

            if (request.PageNumber <= 0)
                request.PageNumber = 1;

            if (request.PageSize <= 0)
                request.PageSize = 1;

            if (request.PageSize > 100)
                request.PageSize = 100;
        }

        public static PaginationResult DefinePaginationResult(PaginationRequest request,  HttpContext httpContext, int total, string endPoint)
        {
            var nextPage = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{endPoint}?PageNumber={request.PageNumber + 1}&PageSize={request.PageSize}";
            var prevPage = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{endPoint}?PageNumber={request.PageNumber - 1}&PageSize={request.PageSize}";

            var pg = new PaginationResult
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (total + (request.PageSize ?? 10) - 1) / (request.PageSize ?? 10),
                TotalReg = total,
                NextPage = request.PageNumber + 1 > total ? null : nextPage,
                PrevPage = request.PageNumber - 1 <= 0 ? null : prevPage,
            };
            return pg;
        }
        
        public static PaginationResult DefinePaginationResult(PaginationRequest request,  HttpContext httpContext, int total, string endPoint, string filters)
        {
            var nextPage = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{endPoint}?PageNumber={request.PageNumber + 1}&PageSize={request.PageSize}&{filters}";
            var prevPage = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{endPoint}?PageNumber={request.PageNumber - 1}&PageSize={request.PageSize}&{filters}";

            var pg = new PaginationResult
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (total + (request.PageSize ?? 10) - 1) / (request.PageSize ?? 10),
                TotalReg = total,
                NextPage = request.PageNumber + 1 > total ? null : nextPage,
                PrevPage = request.PageNumber - 1 <= 0 ? null : prevPage,
            };
            return pg;
        }
    }
}