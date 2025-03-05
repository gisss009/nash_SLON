from flask import Flask, request, jsonify
from db import *

app = Flask(__name__)


@app.route("/users/find_profile")
def find_profile_handler():
    username = request.args.get('username')

    if not username:
        return jsonify({"ok": False, "response": "parameter username is missing."})
    

    return jsonify({"ok": True, "response": find_profile(username)}), 400


@app.route("/users/get_profile")
def get_profile_handler():
    username = request.args.get('username')

    if not username:
        return jsonify({"ok": False, "response": "parameter username is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    profile = get_profile(username)
    ans = {}
    ans["username"] = profile[0]
    ans["name"] = profile[1]
    ans["categories"] = json.loads(profile[2])
    ans["tags"] = json.loads(profile[3])
    ans["own_events"] = json.loads(profile[4])
    ans["events"] = json.loads(profile[5])
    ans["resume"] = profile[6]
    ans["swiped_users"] = json.loads(profile[7])
    ans["mail"] = profile[8]

    return jsonify({"ok": True, "response": ans}), 400


@app.route("/users/add_profile")
def add_profile_handler():
    username = request.args.get('username')

    if not username:
        return jsonify({"ok": False, "response": "parameter username is missing."})

    if find_profile(username):
        return jsonify({"ok": False, "response": "username is alreay exist."})

    add_profile(username)

    return jsonify({"ok": True}), 400


@app.route("/users/edit_profile_name")
def edit_profile_name_handler():
    username = request.args.get('username')
    name = request.args.get('name')

    if not username or not name:
        return jsonify({"ok": False, "response": "parameter username or name is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    edit_profile_name(username, name)

    return jsonify({"ok": True}), 400


@app.route("/users/add_profile_category")
def add_profile_category_handler():
    username = request.args.get('username')
    category = request.args.get('category')

    if not username or not category:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    if category not in ["IT", "Social", "Business", "Health", "Creation", "Sport", "Education", "Science"]:
        return jsonify({"ok": False, "response": "category not in accessed categories"})

    add_profile_category(username, category)

    return jsonify({"ok": True}), 400


@app.route("/users/delete_profile_category")
def delete_profile_category_handler():
    username = request.args.get('username')
    category = request.args.get('category')

    if not username or not category:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    if category not in ["IT", "Social", "Business", "Health", "Creation", "Sport", "Education", "Science"]:
        return jsonify({"ok": False, "response": "category not in accessed categories"})

    delete_profile_category(username, category)

    return jsonify({"ok": True}), 400


@app.route("/users/add_profile_event")
def add_profile_event_handler():
    username = request.args.get('username')
    id = request.args.get('id')

    if not username or not id:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    try:
        id = int(id)
    except ValueError:
        return jsonify({"ok": False, "response": "parameter id is not integer."})

    add_profile_event(username, id)

    return jsonify({"ok": True}), 400


@app.route("/users/delete_profile_event")
def delete_profile_event_handler():
    username = request.args.get('username')
    id = request.args.get('id')

    if not username or not id:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    try:
        id = int(id)
    except ValueError:
        return jsonify({"ok": False, "response": "parameter id is not integer."})

    delete_profile_event(username, id)

    return jsonify({"ok": True}), 400


@app.route("/users/add_profile_own_event")
def add_profile_own_event_handler():
    username = request.args.get('username')
    id = request.args.get('id')

    if not username or not id:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    try:
        id = int(id)
    except ValueError:
        return jsonify({"ok": False, "response": "parameter id is not integer."})

    add_profile_own_event(username, id)

    return jsonify({"ok": True}), 400


@app.route("/users/delete_profile_own_event")
def delete_profile_own_event_handler():
    username = request.args.get('username')
    id = request.args.get('id')

    if not username or not id:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    try:
        id = int(id)
    except ValueError:
        return jsonify({"ok": False, "response": "parameter id is not integer."})

    delete_profile_own_event(username, id)

    return jsonify({"ok": True}), 400


@app.route("/users/edit_profile_resume")
def edit_profile_resume_handler():
    username = request.args.get('username')
    resume = request.args.get('resume')

    if not username or not resume:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    edit_profile_resume(username, resume)

    return jsonify({"ok": True}), 400


@app.route("/users/add_profile_swiped_user")
def add_profile_swiped_user_handler():
    username = request.args.get('username')
    id = request.args.get('id')

    if not username or not id:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    try:
        id = int(id)
    except ValueError:
        return jsonify({"ok": False, "response": "parameter id is not integer."})

    add_profile_swiped_user(username, id)

    return jsonify({"ok": True}), 400


@app.route("/users/delete_profile_swiped_user")
def delete_profile_swiped_user_handler():
    username = request.args.get('username')
    id = request.args.get('id')

    if not username or not id:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    try:
        id = int(id)
    except ValueError:
        return jsonify({"ok": False, "response": "parameter id is not integer."})

    delete_profile_swiped_user(username, id)

    return jsonify({"ok": True}), 400


@app.route("/users/edit_profile_mail")
def edit_profile_mail_handler():
    username = request.args.get('username')
    mail = request.args.get('mail')

    if not username or not id:
        return jsonify({"ok": False, "response": "parameter username or category is missing."})

    if not find_profile(username):
        return jsonify({"ok": False, "response": "username with current username not found."})

    if is_email_exist(mail):
        return jsonify({"ok": False, "response": "email is already exist."})

    edit_profile_mail(username, mail)

    return jsonify({"ok": True}), 400


@app.route("/events/get_event")
def get_event_handler():
    hash = request.args.get('hash')

    if not hash:
        return jsonify({"ok": False, "response": "parameter hash is missing."})

    if not find_event(hash):
        return jsonify({"ok": False, "response": "hash with current hash not found."})

    event = get_event(hash)
    ans = {}
    ans["hash"] = event[0]
    ans["name"] = event[1]
    ans["categories"] = json.loads(event[2])
    ans["description"] = event[3]
    ans["location"] = event[4]
    ans["date_from"] = event[5]
    ans["date_to"] = event[6]

    return jsonify({"ok": True, "response": ans}), 400


@app.route("/events/add_event")
def add_event_handler():
    args = [
        request.args.get('name'),
        request.args.get('categories'),
        request.args.get('description'),
        request.args.get('location'),
        request.args.get('date_from'),
        request.args.get('date_to')
    ]

    for arg in args:
        if not arg:
            return jsonify({"ok": False, "response": f"One of parameters is missing."})
        
    new_event_hash = add_event(args[0], "[]", args[2], args[3], args[4], args[5])
    
    for category in args[1].split(","):
        if category in ["IT", "Social", "Business", "Health", "Creation", "Sport", "Education", "Science"]:
            add_event_category(new_event_hash, category)

    return jsonify({"ok": True}), 400


@app.route("/events/edit_event_name")
def edit_event_name_handler():
    hash = request.args.get('hash')
    name = request.args.get('name')

    if not hash or not name:
        return jsonify({"ok": False, "response": "parameter hash or name is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})
        
    edit_event_name(hash, name)

    return jsonify({"ok": True}), 400


@app.route("/events/add_event_category")
def add_event_category_handler():
    hash = request.args.get('hash')
    category = request.args.get('category')

    if not hash or not category:
        return jsonify({"ok": False, "response": "parameter hash or category is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})

    add_event_category(hash, category)

    return jsonify({"ok": True}), 400


@app.route("/events/delete_event_category")
def delete_event_category_handler():
    hash = request.args.get('hash')
    category = request.args.get('category')

    if not hash or not category:
        return jsonify({"ok": False, "response": "parameter hash or category is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})

    delete_event_category(hash, category)

    return jsonify({"ok": True}), 400


@app.route("/events/edit_event_description")
def edit_event_description_handler():
    hash = request.args.get('hash')
    description = request.args.get('description')

    if not hash or not description:
        return jsonify({"ok": False, "response": "parameter hash or description is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})

    edit_event_description(hash, description)

    return jsonify({"ok": True}), 400


@app.route("/events/edit_event_location")
def edit_event_location_handler():
    hash = request.args.get('hash')
    location = request.args.get('location')

    if not hash or not location:
        return jsonify({"ok": False, "response": "parameter hash or location is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})

    edit_event_location(hash, location)

    return jsonify({"ok": True}), 400


@app.route("/events/edit_event_date")
def edit_event_date_handler():
    hash = request.args.get('hash')
    date_from = request.args.get('date_from')
    date_to = request.args.get('date_to')

    if not hash or not date_from or not date_to:
        return jsonify({"ok": False, "response": "one of the parameters is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})

    edit_event_location(hash, date_from, date_to)

    return jsonify({"ok": True}), 400


@app.route("/users")
def hello_world():
    return """
example of request:<br><br>
for find_profile(username: str): <br>
/users/find_profile?username=diamantove<br>
answer: {"ok": true, "response": true}<br><br>
for add_profile_category(username: str, category: str):<br>
/users/add_profile_category?username=diamantove&category=IT<br>
answer: {"ok": true}
<hr>
find_profile(username: str) <br>
response: bool<br>
<hr>
get_profile(username: str)<br>
response: {<br>
    "username": str,<br>
    "name" : str,<br>
    "categories": list[int],<br>
    "tags": {str (name of category): list[str] (tags of this category), ...},<br>
    "own_events": list[str],<br>
    "events": list[str],<br>
    "resume": str,<br>
    "swiped_users": list[int],<br>
    "mail": str<br>
    }<br>
<hr>
add_profile(username: str)<br>
response: bool<br>
<hr>
edit_profile_name(username: str, name: str)<br>
response: bool<br>
<hr>
add_profile_category(username: str, category: str)<br>
response: bool<br>
<hr>
delete_profile_category(username: str, category: str)<br>
response: bool<br>
<hr>
add_profile_event(username: str, hash: str)<br>
response: bool<br>
<hr>
delete_profile_event(username: str, hash: str)<br>
response: bool<br>
<hr>
add_profile_own_event(username: str, hash: str)<br>
response: bool<br>
<hr>
delete_profile_own_event(username: str, hash: str)<br>
response: bool<br>
<hr>
edit_profile_resume(username: str, resume: str)<br>
response: bool<br>
<hr>
add_profile_swiped_user(username: str, id: int)<br>
response: bool<br>
<hr>
delete_profile_swiped_user(username: str, id: int)<br>
response: bool<br>
<hr>
edit_profile_mail(username: str, mail: int)<br>
response: bool<br>
<hr>
get_event(hash: str)<br>
response: {<br>
    "hash": str,<br>
    "name": str,<br>
    "categories": list[str],<br>
    "description": str,<br>
    "location": str,<br>
    "date_from": int,<br>
    "date_to": int<br>
}<br>
<hr>
add_event(name, categories, description, location, date_from, date_to)<br>
example of categories: IT,Business<br>
date in unixtime<br>
response: bool<br>
<hr>
edit_event_name(hash: str, name: str)<br>
response: bool<br>
<hr>
add_event_category(hash: str, category: str)<br>
response: bool<br>
<hr>
delete_event_category(hash: str, category: str)<br>
response: bool<br>
<hr>
edit_event_description(hash: str, description: str)<br>
response: bool<br>
<hr>
edit_event_location(hash: str, location: str)<br>
response: bool<br>
<hr>
edit_event_date(hash: str, date_from: int, date_to: int)<br>
response: bool<br>
<hr>
"""


@app.route("/events")
def events_handler():
    return """"""


if __name__ == '__main__':
    app.run(debug=True)