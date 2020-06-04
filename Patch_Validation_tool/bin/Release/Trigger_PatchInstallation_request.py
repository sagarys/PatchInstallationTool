#!/usr/bin/env python
import os
import requests
import sys
import pprint
import subprocess
import json

with open(sys.argv[1]) as f:
    example_json = f.read()

try:
    GMproductDetails = json.loads(example_json)
except ValueError as e:
    print("Error while validating your JSON: {}".format(e))
    sys.exit(3)

calculus_req_json={
"request" : {
    "name"                  : "",
    "email_list"            : "",
    "region"                : "vCommander IDC",
    "user"                  : "sagars",
  }
}

install_req_json={
"installs" : [
      {
        "product"             : "",
        "installer_url"       : ""
      }
    ],
}
tests_suite_json = {
"tests" : [
      {
        "product"             : "",
        "timeout_seconds"     : 6000,
      }
    ]
}

patch_req_json = {
        "suite"               : 
        [
      
        ] 
}

def updatePatchTestSuite(prodname,prereqList):
    patchLocation = "-z1 -f /efi/pdlfiles/eng/Sustaining_Patches/"+"\\prodname"
    temp = prereqList.split(',')
    for patch in temp:
        patch_req_json['suite'].append({"exe":"testFierys","timeout_seconds" : 8000,"arguments":"-z1 -f /efi/pdlfiles/eng/Sustaining_Patches/"+prodname+"/"+patch})
        patch_req_json['suite'].append({"exe":"reboot", "timeout_seconds":8000})
        patch_req_json['suite'].append({"exe":"wait_ready", "timeout_seconds":8000})
    tests_suite_json['tests'][0].update(patch_req_json)
        
def checkpreq(podname,prereqList) :
    patch_dir= "\\\\pdlfiles-ba\\pdlfiles\\eng\\Sustaining_Patches"
    patchList = os.listdir(os.path.join(patch_dir,podname))
    prereqListArr = prereqList.split(',')
    print(prereqListArr)
    for prereq in prereqListArr :
        if prereq not in patchList :
            sys.exit(3)
            
checkpreq(GMproductDetails['Product'],GMproductDetails['Prerequisite'])
updatePatchTestSuite(GMproductDetails['Product'],GMproductDetails['Prerequisite'])

ss = json.dumps(calculus_req_json)
calculus_req = json.loads(ss)

ss = json.dumps(install_req_json)
install_req = json.loads(ss)

ss = json.dumps(tests_suite_json)
tests_suite_req = json.loads(ss)

calculus_req_json['request']['email_list'] = GMproductDetails['Email']
if GMproductDetails['IP_Adress'] != "":
    install_req_json['installs'][0].update({"target_ip":GMproductDetails['IP_Adress']})
else:
    install_req_json['installs'][0].update({"livelink":"true"})
    if GMproductDetails['Language'] == "Japanese" and "Windows 10"in GMproductDetails['osType'] :
        install_req_json['installs'][0].update({"vm_template":"Calculus win1064.5.2.1.2 J"})

calculus_req_json['request']['name'] = GMproductDetails['Product']
install_req_json['installs'][0]['product'] = str(GMproductDetails['calculus_name']).replace(" ","")
install_req_json['installs'][0]['installer_url'] = str(GMproductDetails['Installer_patch'])
tests_suite_json['tests'][0]['product'] = str(GMproductDetails['calculus_name']).replace(" ","")

if GMproductDetails['InstallOnServer'] == "True" :
    calculus_req_json['request'].update(install_req_json)

if GMproductDetails['InstallOnServer'] == "False" :
    tests_suite_json['tests'][0].update({"target_ip":GMproductDetails['IP_Adress']})
    calculus_req_json['request'].update(tests_suite_json)

f = open("cal_req.json", "w")
f.write(json.dumps(calculus_req_json))
f.close()
retStatus = subprocess.call("python apiv10.py cal_req.json") 
if retStatus == 1 :
    sys.exit(1)