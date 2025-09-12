// IconFilers.Domain/Entities/ClientDocument.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace IconFilers.Domain.Entities
{
    public class ClientDocument
    {
        [Key]
        public int Id { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        [MaxLength(150)]
        public string? DocType { get; set; }

        [Required, MaxLength(512)]
        public string Filename { get; set; } = null!;

        [MaxLength(100)]
        public string? Mime { get; set; }

        [MaxLength(2000)]
        public string? StoragePath { get; set; }

        public Guid? UploadedBy { get; set; }
        public User? Uploader { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
