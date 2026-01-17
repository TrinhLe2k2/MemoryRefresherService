using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Location.Common
{
    public class BaseResponse<T>
    {
        public BaseResponse()
        { }

        public BaseResponse(T data)
        {
            Code = 200;
            Result = true;
            Data = data;
            Message = "Success";
        }

        public BaseResponse(T? data, string code, string message)
        {
            _ = int.TryParse(code, out int c);
            Code = c;
            Result = c == 200;
            Data = data;
            Message = message;
        }

        public bool Result { get; set; }

        public int Code { get; set; }

        public T? Data { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
