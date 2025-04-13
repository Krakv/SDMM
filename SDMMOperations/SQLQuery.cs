
using DocumentFormat.OpenXml.Wordprocessing;
using System.Threading.Tasks;

namespace SDMMOperations
{
    public static class SQLQuery
    {
        public static MySQLConnector con = new MySQLConnector();


        public static async Task<string> AddDocument(string project_id, string type_id, string author, string status, string size)
        {
            string query = "INSERT INTO document " +
                "(project_id, type_id, author, date, status, size) " +
                "VALUES (@project_id, @type_id, @author, @date, @status, @size); ";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){ 
                { "@project_id", project_id },
                { "@type_id", type_id },
                { "@author", author },
                { "@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "@status", status },
                { "@size", size }
            });

            await task; return task.Result;
        }

        public static async Task<string> AddProject(string name)
        {
            string query = "INSERT INTO project " +
                "(name) " +
                "VALUES (@name); ";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@name", name }
            });

            await task; return task.Result;
        }

        public static async Task<string> AddVersion(string document_id, string version)
        {
            string query = "INSERT INTO version " +
                "(document_id, version, create_date) " +
                "VALUES (@document_id, @version, @create_date); ";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@document_id", document_id },
                { "@version", version },
                { "@create_date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
            });

            await task; return task.Result;
        }

        public static async Task<string> AddSection(string name, string style, string text)
        {
            string query = "INSERT INTO section " +
                "(name, style, text) " +
                "VALUES (@name, @style, @text); ";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@name", name },
                { "@style", style },
                { "@text", text }
            });

            await task; return task.Result;
        }

        public static async Task<string> AddTag(string name)
        {
            string query = "INSERT INTO tag " +
                "(name) " +
                "VALUES (@name); ";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@name", name }
            });

            await task; return task.Result;
        }
        
        public static async Task<string> AddComment(string version_section_id, string text, string user)
        {
            string query = "INSERT INTO comment " +
                "(version_section_id, text, user) " +
                "VALUES (@version_section_id, @text, @user); ";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@version_section_id", version_section_id },
                { "@text", text },
                { "@user", user }
            });

            await task; return task.Result;
        }

        #region update

        public static async Task<string> UpdateComment(string id, string text, string user)
        {
            string query = "UPDATE comment " +
                "SET text = @text, user = @user " +
                "WHERE id = @id " +
                "; ";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@text", text },
                { "@user", user },
                { "@id", id }
            });

            await task; return task.Result;
        }
        
        public static async Task<string> UpdateVersion(string id, string version)
        {
            string query = "UPDATE version " +
                "SET version = @version " +
                "WHERE id = @id " +
                "; ";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@version", version },
                { "@id", id }
            });

            await task; return task.Result;
        }
        
        public static async Task<string> UpdateDocument(string id, string project_id, string type_id, string author, string status, string size)
        {
            string query = "UPDATE document " +
                "SET project_id = @project_id, type_id = @type_id, author = @author, status = @status, size = @size, date = @date " +
                "WHERE id = @id " +
                "; ";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@project_id", project_id },
                { "@type_id", type_id },
                { "@author", author },
                { "@status", status },
                { "@size", size },
                { "@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "@id", id }
            });

            await task; return task.Result;
        }

        public static async Task<string> UpdateVersionNSection(string version_section_id, string version_id, string section_id)
        {
            string query = "UPDATE version_section " +
                "SET version_id = @version_id, section_id = @section_id " +
                "WHERE id = @version_section_id;";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@version_section_id", version_section_id },
                { "@version_id", version_id },
                { "@section_id", section_id }
            });

            await task; return task.Result;
        }


        #endregion update

        public static async Task<string> ConnectVersionNSection(string version_id, string section_id)
        {
            string query = "INSERT INTO version_section " +
                "(version_id, section_id) " +
                "VALUES (@version_id, @section_id);";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@version_id", version_id },
                { "@section_id", section_id }
            });

            await task; return task.Result;
        }
        
        public static async Task<string> ConnectDocumentNTag(string document_id, string tag_id)
        {
            string query = "INSERT INTO document_tag " +
                "(document_id, tag_id) " +
                "VALUES (@document_id, @tag_id);";

            var task = con.NonQueryAsync(query, new Dictionary<string, string>(){
                { "@document_id", document_id },
                { "@tag_id", tag_id }
            });

            await task; return task.Result;
        }

        public static async Task<List<Dictionary<string, string>>> ReadProjects()
        {
            string query = "SELECT * FROM project " +
                ";";

            var task = con.QueryAsync(query);

            await task; return task.Result;
        }

        public static async Task<List<Dictionary<string, string>>> GetProject(string project_id)
        {
            string query = "SELECT * FROM project " +
                "WHERE id = @project_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@project_id", project_id }
            });

            await task; return task.Result;
        }

        public static async Task<List<Dictionary<string, string>>> ReadDocuments(string project_id)
        {
            string query = "SELECT * FROM document " +
                "WHERE project_id = @project_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@project_id", project_id }
            });

            await task; return task.Result;
        }
        
        public static async Task<List<Dictionary<string, string>>> FindDocuments(string text)
        {
            string query = "SELECT document.id, document.project_id, document.type_id, " +
                "document.author, document.date, document.status, document.size, " +
                "tag.name AS \"tag_name\", project.name AS \"project_name\", document_type.name AS \"document_type_name\",  " +
                "document_type.description, standart.name AS \"standart_name\" " +
                "FROM document, document_tag, tag, project, document_type, standart " +
                "WHERE document.type_id = document_type.id " +
                "AND document.id = document_tag.document_id " +
                "AND document_tag.tag_id = tag.id " +
                "AND document.project_id = project.id " +
                "AND document_type.standart_id = standart.id " +
                "AND (project.name LIKE @text OR document_type.name LIKE @text " +
                "OR document.author LIKE @text OR document.status LIKE @text " +
                "OR document.date LIKE @text OR document_type.description LIKE @text " +
                "OR standart.name LIKE @text OR tag.name LIKE @text ) " +
                "ORDER BY project.name " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@text", $"%{text}%" }
            });

            await task; return task.Result;
        }

        public static async Task<List<Dictionary<string, string>>> GetDocument(string document_id)
        {
            string query = "SELECT * FROM document " +
                "WHERE id = @document_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@document_id", document_id }
            });

            await task; return task.Result;
        }

        public static async Task<List<Dictionary<string, string>>> ReadTags(string document_id)
        {
            string query = "SELECT document_tag.id AS \"document_tag_id\", document_id, tag_id, tag.id, name " +
                "FROM document_tag, tag " +
                "WHERE tag.id = document_tag.tag_id " +
                "AND document_tag.document_id = @document_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@document_id", document_id }
            });

            await task; return task.Result;
        }


        public static async Task<List<Dictionary<string, string>>> ReadDocumentTypes(string standart_id)
        {
            string query = "SELECT * FROM document_type " +
                "WHERE standart_id = @standart_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@standart_id", standart_id }
            });

            await task; return task.Result;
        }

        public static async Task<List<Dictionary<string, string>>> GetDocumentTypeInfo(string type_id)
        {
            string query = "SELECT * FROM document_type " +
                "WHERE id = @type_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@type_id", type_id }
            });

            await task; return task.Result;
        }

        public static async Task<List<Dictionary<string, string>>> ReadVersions(string document_id)
        {
            string query = "SELECT * FROM version " +
                "WHERE document_id = @document_id " +
                "ORDER BY create_date DESC " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@document_id", document_id }
            });

            await task; return task.Result;
        }

        public static async Task<List<Dictionary<string, string>>> ReadSections(string version_id)
        {
            string query = "SELECT section.id, section.name, section.style, section.text, version_section.id AS \"version_section.id\", version_section.version_id , version_section.section_id " +
                "FROM section, version_section " +
                "WHERE section.id = version_section.section_id " +
                "AND version_section.version_id = @version_id " +
                "ORDER BY version_section.id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@version_id", version_id }
            });

            await task; return task.Result;
        }
        
        public static async Task<List<Dictionary<string, string>>> ReadComment(string version_section_id)
        {
            string query = "SELECT * " +
                "FROM comment " +
                "WHERE version_section_id = @version_section_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@version_section_id", version_section_id }
            });

            await task; return task.Result;
        }

        public static async Task<List<Dictionary<string, string>>> GetLastVersion(string document_id)
        {
            string query = "SELECT * FROM version " +
                "WHERE document_id = @document_id " +
                "AND create_date = (SELECT MAX(create_date) FROM version WHERE document_id = @document_id) " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@document_id", document_id }
            });

            await task; return task.Result;
        }
        
        public static async Task<List<Dictionary<string, string>>> GetVersion(string version_id)
        {
            string query = "SELECT * FROM version " +
                "WHERE id = @version_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@version_id", version_id }
            });

            await task; return task.Result;
        }
        
        public static async Task<List<Dictionary<string, string>>> GetDocumentName(string document_id)
        {
            string query = "SELECT document_type.name AS document_type_name, project.name AS project_name " +
                "FROM document, document_type, project " +
                "WHERE document.type_id = document_type.id " +
                "AND document.project_id = project.id " +
                "AND document.id = @document_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@document_id", document_id }
            });

            await task; return task.Result;
        }
        
        public static async Task<List<Dictionary<string, string>>> GetTemplate(string template_id)
        {
            string query = "SELECT * FROM template " +
                "WHERE id = @template_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@template_id", template_id }
            });

            await task; return task.Result;
        }
        
        public static async Task<List<Dictionary<string, string>>> ReadTemplates()
        {
            string query = "SELECT * FROM template " +
                ";";

            var task = con.QueryAsync(query);

            await task; return task.Result;
        }

        public static async Task<List<Dictionary<string, string>>> ReadStandarts()
        {
            string query = "SELECT * FROM standart";

            var task = con.QueryAsync(query);

            await task; return task.Result;
        }

        public static async Task<string> FindProject(string name)
        {
            string query = "SELECT * FROM project " +
                "WHERE project.name = @name " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@name", name }
            });

            await task;

            var res = task.Result;

            if (res.Count > 0)
                return res[0]["id"];
            else
                return "";
        }
        
        public static async Task<string> FindComment(string version_id, string section_id)
        {
            string query = "SELECT * FROM comment, version_section " +
                "WHERE version_section.id = comment.version_section_id " +
                "AND version_id = @version_id " +
                "AND section_id = @section_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@version_id", version_id },
                { "@section_id", section_id }
            });

            await task;

            var res = task.Result;

            if (res.Count > 0)
                return res[0]["id"];
            else
                return "";
        }
        
        public static async Task<string> FindComment(string version_section_id)
        {
            string query = "SELECT * FROM comment, version_section " +
                "WHERE version_section.id = comment.version_section_id " +
                "AND version_section_id = @version_section_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@version_section_id", version_section_id }
            });

            await task;

            var res = task.Result;

            if (res.Count > 0)
                return res[0]["id"];
            else
                return "";
        }
        
        public static async Task<string> FindVersionNSectionConnection(string version_id, string section_id)
        {
            string query = "SELECT * FROM version_section " +
                "WHERE version_id = @version_id " +
                "AND section_id = @section_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@version_id", version_id },
                { "@section_id", section_id }
            });

            await task;

            var res = task.Result;

            if (res.Count > 0)
                return res[0]["id"];
            else
                return "";
        }

        public static async Task<List<Dictionary<string, string>>> GetVersionNSectionConnection(string version_section_id)
        {
            string query = "SELECT * FROM version_section " +
                "WHERE id = @version_section_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@version_section_id", version_section_id }
            });

            await task; return task.Result;
        }
        
        public static async Task<List<Dictionary<string, string>>> GetSection(string section_id)
        {
            string query = "SELECT * FROM section " +
                "WHERE id = @section_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@section_id", section_id }
            });

            await task; return task.Result;
        }

        public static async Task<string> FindTag(string name)
        {
            string query = "SELECT * FROM tag " +
                "WHERE tag.name = @name " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@name", name }
            });

            await task;

            var res = task.Result;

            if (res.Count > 0)
                return res[0]["id"];
            else
                return "";
        }

        public static async Task DeleteDocument(string document_id)
        {
            string query = "DELETE FROM document " +
                "WHERE id = @document_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@document_id", document_id }
            });

            await task; return;
        }
        
        public static async Task DeleteVersion(string version_id)
        {
            string query = "DELETE FROM version " +
                "WHERE id = @version_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@version_id", version_id }
            });

            await task; return;
        }
        
        public static async Task DeleteSection(string section_id)
        {
            string query = "DELETE FROM section " +
                "WHERE id = @section_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@section_id", section_id }
            });

            await task; return;
        }
        
        public static async Task DeleteVersionNSectionConnection(string version_id, string section_id)
        {
            string query = "DELETE FROM version_section " +
                "WHERE version_id = @version_id " +
                "AND section_id = @section_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@version_id", version_id },
                { "@section_id", section_id }
            });

            await task; return;
        }
        
        public static async Task DeleteVersionNSectionConnection(string version_section_id)
        {
            string query = "DELETE FROM version_section " +
                "WHERE id = @version_section_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@version_section_id", version_section_id }
            });

            await task; return;
        }
        
        public static async Task DeleteDocumentNTagConnection(string document_id, string tag_id)
        {
            string query = "DELETE FROM document_tag " +
                "WHERE document_id = @document_id " +
                "AND tag_id = @tag_id " +
                ";";

            var task = con.QueryAsync(query, new Dictionary<string, string>(){
                { "@document_id", document_id },
                { "@tag_id", tag_id }
            });

            await task; return;
        }

    }
}
