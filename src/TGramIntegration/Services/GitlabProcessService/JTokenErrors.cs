using System;
using System.Collections.Generic;
using System.Text;

namespace TGramWeb.Services.GitlabProcessService
{
    internal class JTokenErrors
    {
        private ICollection<string> errors;

        public bool HasAny => this.errors != null;

        public void Add(string error)
        {
            if (this.errors == null)
                this.errors = new LinkedList<string>();
            this.errors.Add(error);
        }

        public string Compose()
        {
            var sb = new StringBuilder();

            var i = 1;
            foreach (string error in this.errors)
            {
                sb.Append(i).Append(". ").Append(error).Append(Environment.NewLine);
                i++;
            }

            return sb.ToString();
        }
    }
}
