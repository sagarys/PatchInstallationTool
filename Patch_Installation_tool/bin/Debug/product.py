import requests
import sys
import os
SAAD = "https://saad-ba.efi.com"

def handle_response_errors(response):
    if response.status_code == 404:
        print("Not Found")
        sys.exit(404)

    if response.status_code == 401:
        print("Access Denied")
        sys.exit(401)

    if response.status_code != 200:
        print("Unhandled error: {}".format(response.status_code))
        sys.exit(401)

headers = {
    'Accept': 'application/json',
    'jira-login' : '',
    'jira-password' : '',
    'saad-api-key' : 'c0a109c2-9c81-4412-8ce8-55e743fe8215',
}

projects_url = "{saad}/api/v0/projects/".format(saad=SAAD)

response = requests.get(projects_url, headers=headers)
projects = response.json()

path_build = "\\\\bauser\\Fiery-products\\Sustaining_builds"
dir_list = os.listdir(path_build) 

patch_dir= "\\\\pdlfiles-ba\\pdlfiles\\eng\\Sustaining_Patches"
patch_list = os.listdir(patch_dir) 

f1 = open("Prod_Patch_List.txt", "w")
patchList = {}

for patch in patch_list:
    prod_name = os.path.join(patch_dir,patch)
    subdir_list = os.listdir(os.path.join(patch_dir,patch))
    patchList.update({patch:''})
    for subdir_file in subdir_list :
        patchList[patch] = patchList[patch] + subdir_file +","
    f1.write(str(patch)+":"+str(patchList[patch])+'\n')

print(patchList)
f1.close()

list = []
for directory in dir_list:
    prod_name = os.path.join(path_build,directory)
    subdir_list = os.listdir(os.path.join(path_build,directory))
    for subdir_file in subdir_list :
        if "GM" in subdir_file :
            list.append(directory)
            
print(projects[0])
#print(list)
f = open("GM_Prod_List.txt", "w")
 
for project_dict in projects:
    for prod in list :
        #print(str("{name}".format(**project_dict)).replace(" ",""))
        if(str("{name}".format(**project_dict)).replace(" ","") == prod) :
             print("{name} : {server os} : {calculus name} : {oem} ".format(**project_dict))
             f.write("{name} : {server os} : {calculus name} : {oem} ".format(**project_dict) + '\n')
            
f.close()
   