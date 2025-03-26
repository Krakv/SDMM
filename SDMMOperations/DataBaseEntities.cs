namespace SDMMOperations
{
    public class DataBaseEntities
    {
        public class Standart
        {
            public  string id;
            public  string name;

            public Standart(string id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public override string ToString()
            {
                return name;
            }
        }
        
        public class Version
        {
            public  string id;
            public  string document_id;
            public  string version;
            public  string create_date;

            public Version(string id, string document_id, string version, string create_date)
            {
                this.id = id;
                this.version = version;
                this.document_id = document_id;
                this.create_date = create_date;
            }

            public override string ToString()
            {
                return version + " | " + create_date;
            }
        }
        
        public class Tag
        {
            public  string id;
            public  string name;

            public Tag(string id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public override string ToString()
            {
                return name;
            }
        }
        
        public class Project
        {
            public  string id;
            public  string name;

            public Project(string id, string name)
            {
                this.id = id;
                this.name = name;
            }

            public override string ToString()
            {
                return name;
            }
        }

        public class Document
        {
            public  string id;
            public  Project project;
            public  DocumentType documentType;
            public  string author;
            public  string date;
            public  string status;
            public  string size;
            public  string path;
            public  string tags;

            public Document(string id, Project project, DocumentType documentType, string author, string date, string status, string size, string path = "", string tags = "")
            {
                this.id = id;
                this.project = project;
                this.documentType = documentType;
                this.author = author;
                this.date = date;
                this.status = status;
                this.size = size;
                this.path = path;
                this.tags = tags;
            }

            public override string ToString()
            {
                return project.name + " - " + documentType.name;
            }
        }

        public class DocumentType
        {
            public  string id;
            public  string name;
            public  string standartID;
            public  string templateID;
            public  string description;

            public DocumentType(string id, string name, string standartID, string templateID, string description)
            {
                this.id = id;
                this.name = name;
                this.standartID = standartID;
                this.templateID = templateID;
                this.description = description;
            }

            public override string ToString()
            {
                return name;
            }
        }

        public class Section
        {
            public  string id;
            public  string name;
            public  string style;
            public  string text;

            public Section(string id, string name, string style, string text)
            {
                this.id = id;
                this.name = name;
                this.style = style;
                this.text = text;
            }

            public override string ToString()
            {
                return name;
            }
        }
    }
}
