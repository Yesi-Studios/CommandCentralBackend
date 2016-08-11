using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCServ.ClientAccess
{
    /// <summary>
    /// An enum describing the possible states a message can be in.
    /// </summary>
    public enum MessageStates
    {
        /// <summary>
        /// Indicates that a request has been received and has not begun processing.  
        /// <para />
        /// Requests in this state have not undergone validation of any type and assumptions regarding their contained data should not be made.
        /// </summary>
        Received,
        /// <summary>
        /// Indicates a message has been processed successfully prior to authentication or method invocation.
        /// </summary>
        Processed,
        /// <summary>
        /// Indicates a message has been successfully authenticated prior to method invocation but after message processing.
        /// </summary>
        Authenticated,
        /// <summary>
        /// Indicates that the message has completed invoking its endpoint's data handler.  The final actions are final logging and response release.
        /// </summary>
        Invoked,
        /// <summary>
        /// Indicates that the message has been processed, possible authenticated, and has passed its data handler.
        /// <para />
        /// Additionally, the request has completed final logging and is only waiting to release the response.
        /// <para />
        /// At this point the message is complete and the response need only be sent back to the client.
        /// </summary>
        Handled,
        /// <summary>
        /// Indicates that we shat the bed so fucking hard that the service collapsed under the weight of the shit in the bed.  This epic amount of shit then tore through the floor boards, collapsed the supports under the floor we were staying on and then slammed straight into the .NET framework (or some other framework).  Freaking out, .NET or another framework screamed a bloody scream. "WHY WOULD YOU DO THAT?!", it exclaimed in dismay. It then balled up all of our shit into a nice neat pile (and there was a lot so you can be sure it's a big ball) and then began the process of HEAVING this big 'ole ball of shit alllll the way back up to the endpoint's entry.  Here it was caught by the catch block of the outer try/catch block.  This experience, from the catch block's point of view, was not unlike that of a mack truck slamming into a deer at 60+ mph.  The deer, for its part, did not move.  It took that bitch right in the face. God bless you Ron.  We couldn't find any piece of you left, but we will love you forever.  You're in our hearts.  After pulverizing the deer, the mack truck unloaded the big pile of shit we made on the second story of that Hotel California and then - bless its heart - packaged up the shit yet again into a different ball.  It sent this pile of shit, by way of email, to the developers who, we hope, still give a shit enough to fix whatever caused the shitting in the first place.  The client, however, saw none of this.  They got a message apologizing that something went wrong.  They. will. never. understand what work. What PAINFUL work went into keeping the whole world from collpasing because we shat in the bed of their request. In the end, fuck you, fuck your bald spot, fuck your glasses, and McLean, fuck your beer belly. Btw, if you're reading this and you're thinking "well this should be removed" you're wrong.  And if you think you're right then Doctor's orders - go fuck yourself. And if you've gotten this far, you must have enjoyed at least a bit of it, soooo, maybe you should leave it here for the rest of those who may be amused by it.  In fact, if you read this (even if you have read it before), you are now responsible for adding at least one sentence to further the story (before this sentence).
        /// </summary>
        FatalError
    }
}
