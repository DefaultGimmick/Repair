﻿using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Repair.Api.Filters
{
    /// <summary>
    /// swagger上传文件接口过滤器
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        /// <summary>
        /// 过滤方法
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="context"></param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var actionDescriptor = context.ApiDescription.ActionDescriptor;
            //判断上传文件的类型，只有上传的类型是IFormCollection的才进行重写。
            if (actionDescriptor.Parameters.All(
                    w => w.ParameterType != typeof(IFormCollection))
                && actionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor ad
                && !ad.ActionName.Contains("UploadFile")
            ) return;

            var schema = new Dictionary<string, OpenApiSchema>
            {
                ["fileName"] = new OpenApiSchema
                {
                    Description = "Select file",
                    Type = "string",
                    Format = "binary"
                }
            };

            var content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema { Type = "object", Properties = schema }
                }
            };

            operation.RequestBody = new OpenApiRequestBody() { Content = content };
        }
    }
}
