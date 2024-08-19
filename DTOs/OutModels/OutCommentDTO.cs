using FlightSearch.DTOs.InModels;

namespace FlightSearch.DTOs.OutModels
{
    public class OutCommentDTO
    {
        public string CommentText { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public OutUserDTO User { get; set; } = new OutUserDTO();
    }
}