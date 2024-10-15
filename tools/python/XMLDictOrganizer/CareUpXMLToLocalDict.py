#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import random
import os
import json
from lxml import etree as ET
import io

xml_directories = ["Xml/Quiz/"]
dump_folder = "Quiz2/"
work_dict_name = "quiz_dict"

res_path = "../../../care-up/Assets/Resources/"
dict_root_path =  res_path + "Dictionaries/Dutch/"
dict_names = []
set_of_dictionaries = []


def load_dict(dict_path):
    with open(dict_path, 'r') as file:
        j_data = json.load(file)
    return j_data


def get_work_dict_index():
    index = -1
    for i in range(len(dict_names)):
        if work_dict_name == dict_names[i]:
            index = i
    if index == -1:
        index = (len(work_dict_name) - 1)
    return index


def save_dict(dict_index):
    json_object = json.dumps(set_of_dictionaries[dict_index], indent = 4, ensure_ascii=False).encode('utf8')
    dump_file_path = dump_folder + dict_names[dict_index] + ".json"
    print(dump_file_path)
    file = io.open(dump_file_path, mode="w", encoding="utf-8")
    file.write(json_object.decode())
    file.close()


def is_it_key(key):
    if (len(key) >= 3):
        if key[0] == "[" and key[-1] == "]":
            return True
    return False
 

def strip_key(key):
    return key[1:-1]


def get_value_if_key(key):
    if is_it_key(key):
        return get_localization_value(strip_key(key))
    return key


def get_localization_value(key):
    for i in range(len(set_of_dictionaries)):
        if (key in set_of_dictionaries[i]):
            return set_of_dictionaries[i][key]
    return ""
    

def find_dict_and_key_by_value(value):
    for i in range(len(set_of_dictionaries)):
        for k in set_of_dictionaries[i]:
            if set_of_dictionaries[i][k] == value:
                return dict_names[i], k
    return "", ""


def key_exist(key):
    for d in set_of_dictionaries:
        if key in d:
            return True
    return False


def clean_text(value):
    new_text = value.replace("!!!NEWLINE!!!", "<br>").replace("\r", "").replace("\"","“")
    return new_text


def list_xml_files_in_dir(_dir):
    result = []
    for file_path in os.listdir(_dir):
        if (file_path.split('.')[-1] == "xml"):
            result.append(_dir + file_path)
    return result

def generate_new_key(value):
    value = value.lower().replace("<br>", "").replace(".", "").replace(",", "").replace("-", "").replace(";", "")
    for i in range(5):
        value = value.replace("  ", " ")
    value = value.strip()
    key_base = value
    split_value = value.split(" ")
    if len(split_value) > 3:
        key_base = split_value[0] + " " + split_value[1] + " " + split_value[2]

    final_key = key_base
    while key_exist(final_key):
        final_key = key_base + str(random.randint(3, 999)) 
    return final_key

for file_path in os.listdir(dict_root_path):
    if (file_path.split('.')[-1] == "json"):
        dict_names.append(file_path.split('.')[0])
        set_of_dictionaries.append(load_dict(dict_root_path + file_path))
print("_____")
print(dict_names)


if not(work_dict_name in dict_names):
    dict_names.append(work_dict_name)
    work_dict = {}
    set_of_dictionaries.append(work_dict)

needed_attr = ["extra", "name", "description", "fullDescription", "messageTitle", "descr", "messageContent", "text", "title", "blockMessage"]
node_names = ["Action", "action", "Scene", "scene", "Option", "option", 
              "Info", "info", "Answer", "answer", "question", "Question"]

xml_files = []
for d in xml_directories:
    xml_files += list_xml_files_in_dir(res_path + d)


for xml_file in xml_files:
    print(xml_file)
    ff = open(xml_file, 'r', encoding='utf-8')
    data = ff.read()
    ff.close()
    new_data = ""
    b_trigger = False
    for i in range(len(data)):
        if (data[i] == "\""):
            b_trigger = not b_trigger
        if data[i] == "\n" and b_trigger:
            pass
            new_data += "!!!NEWLINE!!!"
        else:
            new_data += data[i]
    tree = ET.ElementTree(ET.fromstring(bytes(new_data, encoding='utf-8')))
    # tree = ET.parse(data)
    root = tree.getroot()

    for n in node_names:
        for node in tree.iter(node_names):
            for k in node.attrib:
                if k in needed_attr:
                    if (node.attrib[k] == ""):
                        continue
                    if not(is_it_key(node.attrib[k])):
                        _value = clean_text(node.attrib[k])
                        found_in_dict, found_key = find_dict_and_key_by_value(_value)
                        if type(found_key) == str and found_key != "":
                            node.attrib[k] = "[" + found_key + "]"
                        else:
                            new_key = generate_new_key(_value)
                            set_of_dictionaries[get_work_dict_index()][new_key] = _value
                            node.attrib[k] = "[" + new_key + "]"

    ET.indent(tree, '  ')

    xml_str = ET.tostring(tree, encoding='unicode', pretty_print=True).replace("\t", "  ")
    new_text = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n"
    buffer_text = ""
    for i in range(len(xml_str)):
        c = xml_str[i]
        buffer_text += c
        if c == ' ':
            new_text += buffer_text
            buffer_text = "" 
        if c == "=":
            buffer_text = "\n      " + buffer_text

    new_text += buffer_text
    xml_file_name = xml_file.split("/")[-1]
    file = open(dump_folder + xml_file_name, "w")
    file.write(new_text)
    file.close()

save_dict(get_work_dict_index())
# save_dict(len(set_of_dictionaries) - 1)


