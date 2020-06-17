#!/usr/bin/env python
import os
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

ss = json.dumps(calculus_req_json)
calculus_req = json.loads(ss)

ss = json.dumps(install_req_json)
install_req = json.loads(ss)

ss = json.dumps(tests_suite_json)
tests_suite_req = json.loads(ss)

def calculusRequest () :
    calculus_req_json['request']['name'] = GMproductDetails['Product']
    calculus_req_json['request']['email_list'] = GMproductDetails['Email']
    calculus_req_json['request']['user'] = GMproductDetails['Email'].split('@')[0]
    
def cffEnable() :
    calculus_req_json['request']['name'] = calculus_req_json['request']['name'] + "_CFF_Enable"
    tests_suite_json['tests'][0]['product'] = str(GMproductDetails['calculus_name']).replace(" ","")
    if GMproductDetails['ServerType'] == 'Server' :
        patch_req_json['suite'].append({"exe":"CFF_Enable","timeout_seconds" : 8000})
        patch_req_json['suite'].append({"exe":"reboot", "timeout_seconds":8000})
        patch_req_json['suite'].append({"exe":"wait_ready", "timeout_seconds":8000})
    tests_suite_json['tests'][0].update({"target_ip":GMproductDetails['IP_Adress']})
    tests_suite_json['tests'][0].update(patch_req_json)
    calculus_req_json['request'].update(tests_suite_json)
        
def updatePatchTestSuite(prodname,prereqList):
    if prereqList != '' :  
        temp = prereqList.split(',')
        tests_suite_json['tests'][0]['product'] = str(GMproductDetails['calculus_name']).replace(" ","")
        for patch in temp:
            patch_req_json['suite'].append({"exe":"testFierys","timeout_seconds" : 8000,"arguments":"-z1 -f /efi/pdlfiles/eng/Sustaining_Patches/"+prodname+"/"+patch})
            patch_req_json['suite'].append({"exe":"reboot", "timeout_seconds":8000})
            patch_req_json['suite'].append({"exe":"wait_ready", "timeout_seconds":8000})
        
        if GMproductDetails['ServerType'] ==  'VM' :
            patch_req_json['suite'].append({"exe":"sleeper", "timeout_seconds":8000})
        
        tests_suite_json['tests'][0].update(patch_req_json)
        calculus_req_json['request'].update(tests_suite_json)
        
def checkpreq(podname,prereqList) :
    patch_dir= "\\\\pdlfiles-ba\\pdlfiles\\eng\\Sustaining_Patches"
    if not os.path.isdir(os.path.join(patch_dir,podname)) and GMproductDetails['Prerequisite'] != '':
        sys.exit(4)
    if not os.path.isdir(os.path.join(patch_dir,podname)) :
        return
    patchList = os.listdir(os.path.join(patch_dir,podname))
    if prereqList != '' :
        prereqListArr = prereqList.split(',')
        if len(prereqListArr) != 0 :
            for prereq in prereqListArr :
                if prereq not in patchList :
                    sys.exit(3)

def installOnServer(installerPath) :
    install_req_json['installs'][0]['product'] = str(GMproductDetails['calculus_name']).replace(" ","")
    install_req_json['installs'][0]['installer_url'] = str(GMproductDetails['Installer_path'])
    install_req_json['installs'][0].update({"target_ip":GMproductDetails['IP_Adress']})
    calculus_req_json['request'].update(install_req_json)

def installOnVM(installerPath) :
    install_req_json['installs'][0]['product'] = str(GMproductDetails['calculus_name']).replace(" ","")
    install_req_json['installs'][0]['installer_url'] = str(GMproductDetails['Installer_path'])
    install_req_json['installs'][0].update({"livelink":"true"})
    calculus_req_json['request'].update(install_req_json)

def checkOSAndlang() :
    if GMproductDetails['ServerType'] ==  'VM' :
        if(GMproductDetails['Language'] == 'Japanese' and "Windows 10" in GMproductDetails['osType']) :
            install_req_json['installs'][0]['vm_template'] = "Calculus win1064.5.2.1.2 J"
    calculus_req_json['request'].update(install_req_json)

checkpreq(GMproductDetails['Product'],GMproductDetails['Prerequisite'])
calculusRequest()
checkOSAndlang()

if GMproductDetails['Enable_CFF'] == 'True' :
    cffEnable()
    f = open("cal_req.json", "w+")
    f.write(json.dumps(calculus_req_json))
    f.close()
    retStatus = subprocess.call("python apiv10.py cal_req.json Enable_CFF") 
    if retStatus == 1 :
        sys.exit(1)
    else :
        sys.exit(0)
        
if GMproductDetails['WithInstaller'] == 'True' :
    if(GMproductDetails['ServerType'] ==  'Server') :
        installOnServer(GMproductDetails['Installer_path'])
    if(GMproductDetails['ServerType'] ==  'VM') :
        installOnVM(GMproductDetails['Installer_path'])
    if GMproductDetails['Prerequisite'] != '' :
        updatePatchTestSuite(GMproductDetails['Product'],GMproductDetails['Prerequisite'])

if GMproductDetails['WithInstaller'] == 'False' :
    if GMproductDetails['Prerequisite'] == '' :
        sys.exit(5)
    else :
        tests_suite_json['tests'][0].update({"target_ip":GMproductDetails['IP_Adress']})
        updatePatchTestSuite(GMproductDetails['Product'],GMproductDetails['Prerequisite'])
    
f = open("cal_req.json", "w+")
f.write(json.dumps(calculus_req_json))
f.close()
retStatus = subprocess.call("python apiv10.py cal_req.json") 
if retStatus == 1 :
    sys.exit(1)