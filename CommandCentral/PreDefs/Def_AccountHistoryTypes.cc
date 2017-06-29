{
  "TypeFullName": "CommandCentral.Entities.ReferenceLists.AccountHistoryType",
  "Definitions": [
    {
      "Id": "64C5B78D-D5FD-496A-9579-D54549A40257",
      "Value": "Creation",
      "Description": "Indicates an account creation event.  This should only occur once."
    },
    {
      "Id": "C38606E8-9A1E-4653-A944-8010B00682C0",
      "Value": "Login",
      "Description": "A routine, successful login."
    },
    {
      "Id": "35D5F49F-9EBD-48BA-A526-26C5596EA508",
      "Value": "Logout",
      "Description": "A logout event."
    },
    {
      "Id": "4AEDD7AC-DCF4-4AEE-98C1-C73FB422CF86",
      "Value": "Failed Login",
      "Description": "A failed attempt to login to the account. Many of these may indicate malicious activity."
    },
    {
      "Id": "7580B641-2059-49FA-8E00-6D44CECE5034",
      "Value": "Registration Started",
      "Description": "The beginning of the registration process, when the user inputs the SSN and receives an email."
    },
    {
      "Id": "CD626AF1-BCFA-448B-992E-4D65A934571B",
      "Value": "Registration Completed",
      "Description": "The end of the registration process, after which the user should have account access."
    },
    {
      "Id": "214B8471-5087-4C64-A91C-2FCC0232CE9F",
      "Value": "Password Reset Initiated",
      "Description": "The beginning of the password reset process, during which the user receives an email with instructions for password reset."
    },
    {
      "Id": "609D71A8-5FB7-44A7-B5E8-242C3BE20079",
      "Value": "Password Reset Completed",
      "Description": "The end of the password reset process."
    },
    {
      "Id": "BA03299F-CA2E-40B2-BBA1-6BC377E4D75C",
      "Value": "Password Changed",
      "Description": "The account password was changed."
    },
    {
      "Id": "80123747-55E3-40B2-A834-E31CBC2DEA72",
      "Value": "Username Forgotten",
      "Description": "The client indicated that he or she forgot the account's username, and it was emailed to the user."
    }
  ]
}