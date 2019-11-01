from robot.libraries.BuiltIn import BuiltIn
s2l = BuiltIn().get_library_instance('SeleniumLibrary')
import re
import requests
import time

mailEndpoint = "http://api.guerrillamail.com/ajax.php"

def get_email_address():
  params = {}
  params['f'] = "get_email_address"
  r = requests.get(mailEndpoint, params=params)
  return (r.cookies['PHPSESSID'], r.json()['email_addr'])

def check_email(session_id):
  params = {}
  params['f'] = "check_email"
  params['sid_token'] = session_id
  params['seq'] = "0"
  return requests.get(mailEndpoint, params=params)

def wait_for_email(session_id, address):
  for i in range(10):
    r = None
    time.sleep(3)
    r = check_email(session_id)
    list = r.json()['list']
    if list != []:
      for email in r.json()['list']:
        # BuiltIn().log_to_console('something was found...')
        if email['mail_from'] == address:
          BuiltIn().log_to_console("Found the email!!!")
          BuiltIn().log_to_console("Subject: %s" % email['mail_subject'])
          BuiltIn().log_to_console("Content: %s" % email['mail_excerpt'])
          return  email['mail_excerpt']
      # BuiltIn().log_to_console('not the email we are looking for')

  raise AssertionError("Email not found!")

def get_confirmation_link(bodyStr):
  url = re.findall('https://notifications-explore-education-statistics-(?:test|stage|live)[a-z0-9./-]*verify-subscription/[-._a-zA-Z0-9]*', bodyStr)
  if not url:
    AssertionError("No confirmation URL found!")
  return url[0]

if __name__ == '__main__':
  # TEST GETTING EMAIL
  session_id, address = get_email_address()
  print("Email address is %s" % address)
  print("Session ID is %s" % session_id)
  wait_for_email(session_id, "mark@hiveit.co.uk")

  # # TEST GETTING CONFIRMATION LINK
  # confirmLink = get_confirmation_link("href=\"https://notifications-explore-education-statistics-test.azurewebsites.net/api/publication/cbbd299f-8297-44bc-92ac-558bcf51f8ad/verify-subscription/eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Im1hcmtAaGl2ZWl0LmNvLnVrIiwiZXhwIjoxNTU2MTk2OTQ5LCJpc3MiOiJTYW1wbGUiLCJhdWQiOiJTYW1wbGUifQ.p9VZ0tmFEkBJ0n5fqy4VgCbV2219PkwInR9K0MrHqpk\"")
  # print(str(confirmLink))
  #
  # confirmLink = get_confirmation_link("href=\"https://notifications-explore-education-statistics-stage.azurewebsites.net/api/publication/bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9/verify-subscription/eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Im1hcmtAaGl2ZWl0LmNvLnVrIiwiZXhwIjoxNTU2MjAwOTU3LCJpc3MiOiJTYW1wbGUiLCJhdWQiOiJTYW1wbGUifQ.SnWEiSeRMfI5_UsJhkMovYvbzawXK3-YxzEIeKPVttM\"")
  # print(str(confirmLink))
  #
  # confirmLink = get_confirmation_link("href=\"https://notifications-explore-education-statistics-live.azurewebsites.net/api/publication/cbbd299f-8297-44bc-92ac-558bcf51f8ad/verify-subscription/eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Im1hcmtAaGl2ZWl0LmNvLnVrIiwiZXhwIjoxNTU2MjAwNzgzLCJpc3MiOiJTYW1wbGUiLCJhdWQiOiJTYW1wbGUifQ.NF7zROcMJ-Yw3W9Qg17ZTXOncBkIeXdjSaiWnKc9ZvE\"")
  # print(str(confirmLink))
