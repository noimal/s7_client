using System;

namespace s7_client {
    public class S7ClientException : Exception {
        public int Code { get; }

        public S7ClientException(int code, string message) : base(message) {
            Code = code;
        }

        public override string ToString() {
            return $"{Code.ToString()} : {Message}";
        }
    }
}
