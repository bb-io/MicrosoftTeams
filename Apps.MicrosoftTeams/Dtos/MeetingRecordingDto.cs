using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apps.MicrosoftTeams.Dtos
{
    public class MeetingRecordingDto
    {
       // public DateTime createdDateTime { get; set; }        
        public string recordingContentUrl { get; set; }

        public MeetingRecordingDto(CallRecording input) 
        {
            // createdDateTime = input.CreatedDateTime ;
            recordingContentUrl = input.RecordingContentUrl;
        }
    }
}
