{
  "TypeFullName": "CommandCentral.Entities.ReferenceLists.Watchbill.WatchbillStatus",
  "Definitions": [
    {
      "Id": "FA1D4185-6A36-40DE-81C6-843E6EE352F0",
      "Value": "Initial",
      "Description": "The watchbill has just been created and its days/shifts are being defined.  It is not open for inputs."
    },
    {
      "Id": "0AD04630-679A-4275-A51B-6F7663BB103E",
      "Value": "Open for Inputs",
      "Description": "The watchbill is now accepting watch inputs from all Sailors or their chains of command."
    },
    {
      "Id": "34092469-541A-40DF-9729-A74396362131",
      "Value": "Closed for Inputs",
      "Description": "The watchbill is no longer accepting watch inputs.  Soon, the it will be populated and released to the watchbill cooridinators for review."
    },
    {
      "Id": "579F6625-3966-446A-85FC-CC6335407D38",
      "Value": "Under Review",
      "Description": "The watchbill has been populated and is currently under review.  Any last minute watch swaps should happen now."
    },
    {
      "Id": "17D7339B-21EC-429E-B4C0-E34E36691521",
      "Value": "Published",
      "Description": "The watchbill has been published to the command.  Changes to it will now require the command watchbill coordinator's intervention."
    }
  ]
}