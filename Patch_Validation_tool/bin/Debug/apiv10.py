#!/usr/bin/env python
import requests
import json
import sys
import os

CALCULUS_HOST='calculus.efi.com'

with open(sys.argv[1]) as f:
    example_json = f.read()

try:
    example_dict = json.loads(example_json)
except ValueError as e:
    print("Error while validating your JSON: {}".format(e))
    sys.exit(1)

headers = {'content-type': 'application/json'}
my_creds = ('mikep', 'topsecret')
# POSTing creates a new Calculus Request
#
r = requests.post("https://{calculus_host}/api/v10/requests.json".format(calculus_host=CALCULUS_HOST), headers=headers, data=json.dumps(example_dict), auth=my_creds, verify=False)
d = r.json()
if 'errors' in d:
    print(d)
    sys.exit(1)
else:
    new_request_id = d['request id']
    print("created new request: {}".format(new_request_id))
    
    # GETting retrieves Calculus Request information in JSON format
    #
    r = requests.get("https://{calculus_host}/api/v10/requests/{id}.json".format(calculus_host=CALCULUS_HOST, id=new_request_id))
    try:
        print(" request name: {}".format(r.json()['request']['name']))
    except ValueError:
        print(r.text)
        sys.exit(1)
