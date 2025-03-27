from flask import Flask, request, jsonify
from db import *
import requests
import os

app = Flask(__name__)

def require_auth(func):
    def wrapper(*args, **kwargs):
        username = request.args.get("from_user_username")
        password = request.args.get("from_user_password")

        if not username or not password:
            return jsonify({"ok": False, "response": "Missing from_user_username or from_user_password."}), 400

        if not is_user_and_password_correct(username, password):
            return jsonify({"ok": False, "response": "Invalid from_user_username or from_user_password."}), 401

        return func(*args, **kwargs)

    wrapper.__name__ = func.__name__
    return wrapper


@app.route("/add_username_and_password")
def add_username_and_password_handler():
    username = request.args.get('username')
    password = request.args.get('password')

    if not username or not password:
        return jsonify({"ok": False, "response": "parameters username or password is missing."})

    if is_exist_username(username):
        return jsonify({"ok": False, "response": "this username is already exists."})

    add_username_and_password(username, password)
    return jsonify({"ok": True}), 400


@app.route("/is_username_and_password_correct", methods=['POST', 'GET'])
def is_username_and_password_handler():
    username = request.args.get('username')
    password = request.args.get('password')

    if not username or not password:
        return jsonify({"ok": False, "response": "parameters username or password is missing."})

    return jsonify({"ok": True, "response": is_user_and_password_correct(username, password)}), 400


@app.route("/users/find_profile")
@require_auth
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


# @app.route("/users/add_profile_own_event")
# def add_profile_own_event_handler():
#     username = request.args.get('username')
#     id = request.args.get('id')

#     if not username or not id:
#         return jsonify({"ok": False, "response": "parameter username or category is missing."})

#     if not find_profile(username):
#         return jsonify({"ok": False, "response": "username with current username not found."})

#     try:
#         id = int(id)
#     except ValueError:
#         return jsonify({"ok": False, "response": "parameter id is not integer."})

#     add_profile_own_event(username, id)

#     return jsonify({"ok": True}), 400


# @app.route("/users/delete_profile_own_event")
# def delete_profile_own_event_handler():
#     username = request.args.get('username')
#     id = request.args.get('id')

#     if not username or not id:
#         return jsonify({"ok": False, "response": "parameter username or category is missing."})

#     if not find_profile(username):
#         return jsonify({"ok": False, "response": "username with current username not found."})

#     try:
#         id = int(id)
#     except ValueError:
#         return jsonify({"ok": False, "response": "parameter id is not integer."})

#     delete_profile_own_event(username, id)

#     return jsonify({"ok": True}), 400


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
        request.args.get('owner_id'),
        request.args.get('categories'),
        request.args.get('description'),
        request.args.get('location'),
        request.args.get('date_from'),
        request.args.get('date_to'),
        request.args.get('public'),
        request.args.get('online')
    ]

    for arg in args:
        if not arg:
            return jsonify({"ok": False, "response": f"One of parameters is missing."})
        
    new_event_hash = add_event(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8])
    
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


@app.route("/events/get_all_events")
def get_all_events_handler():
    return jsonify({"ok": True, "response": get_all_events()}), 400

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
&nbsp;&nbsp;&nbsp;&nbsp; "username": str,<br>
&nbsp;&nbsp;&nbsp;&nbsp; "name" : str,<br>
&nbsp;&nbsp;&nbsp;&nbsp; "categories": list[int],<br>
&nbsp;&nbsp;&nbsp;&nbsp; "tags": {str (name of category): list[str] (tags of this category), ...},<br>
&nbsp;&nbsp;&nbsp;&nbsp; "own_events": list[str],<br>
&nbsp;&nbsp;&nbsp;&nbsp; "events": list[str],<br>
&nbsp;&nbsp;&nbsp;&nbsp; "resume": str,<br>
&nbsp;&nbsp;&nbsp;&nbsp; "swiped_users": list[int],<br>
&nbsp;&nbsp;&nbsp;&nbsp; "mail": str<br>
&nbsp;&nbsp;&nbsp;&nbsp; }<br>
<hr>
add_profile(username: str)<br>
<hr>
edit_profile_name(username: str, name: str)<br>
<hr>
add_profile_category(username: str, category: str)<br>
<hr>
delete_profile_category(username: str, category: str)<br>
<hr>
add_profile_event(username: str, hash: str)<br>
<hr>
delete_profile_event(username: str, hash: str)<br>
<hr>
add_profile_own_event(username: str, hash: str)<br>
<hr>
delete_profile_own_event(username: str, hash: str)<br>
<hr>
edit_profile_resume(username: str, resume: str)<br>
<hr>
add_profile_swiped_user(username: str, id: int)<br>
<hr>
delete_profile_swiped_user(username: str, id: int)<br>
<hr>
edit_profile_mail(username: str, mail: int)<br>
<hr>
get_profiles_by_categories(categories: str,str,str...)<br>
response: list[dict]<br>
! The tags contain the tags of the requested categories
example:
[<br>
&nbsp;&nbsp;&nbsp;&nbsp; {<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "username": "user1",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "name": "Alice",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "categories": "IT,Health",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "tags": ["Python", "SQL"],<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "events": "event1,event2",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "resume": "Resume1",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "swiped_users": "user2,user3",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "mail": "alice@example.com"<br>
&nbsp;&nbsp;&nbsp;&nbsp; },<br>
&nbsp;&nbsp;&nbsp;&nbsp; {<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "username": "user2",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "name": "Bob",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "categories": "Business,Sport",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "tags": ["Football", "Tennis"],<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "events": "event3",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "resume": "Resume2",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "swiped_users": "user1",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;  "mail": "bob@example.com"<br>
&nbsp;&nbsp;&nbsp;&nbsp; },<br>
]<br>
<hr>

"""


@app.route("/events")
def events_handler():
    return """
get_event(hash: str)<br>
response: {<br>
&nbsp;&nbsp;&nbsp;&nbsp; "hash": str,<br>
&nbsp;&nbsp;&nbsp;&nbsp; "name": str,<br>
&nbsp;&nbsp;&nbsp;&nbsp; "categories": list[str],<br>
&nbsp;&nbsp;&nbsp;&nbsp; "description": str,<br>
&nbsp;&nbsp;&nbsp;&nbsp; "location": str,<br>
&nbsp;&nbsp;&nbsp;&nbsp; "date_from": int,<br>
&nbsp;&nbsp;&nbsp;&nbsp; "date_to": int<br>
}<br>
<hr>
add_event(name: str, owner_id: int, categories: str,str,..., description: str, location: str, date_from: str, date_to: str, public: 0 or 1, online: 0 or 1)<br>
example of categories: IT,Business<br>
<hr>
get_events_by_categories(categories: str,str...)<br>
response: list[dict]<br>
example:<br>
[<br>
&nbsp;&nbsp;&nbsp;&nbsp; {<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "hash": "abc123",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "owner_id": 1,<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "categories": ["IT","Health","Creation"],<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "name": "Event1",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "description": "Desc1",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "location": "Loc1",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "date_from": "2023-01-01",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "date_to": "2023-01-05",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "public": 1,<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "online": 0<br>
&nbsp;&nbsp;&nbsp;&nbsp; },<br>
]<br>
<hr>
edit_event_name(hash: str, name: str)<br>
<hr>
add_event_category(hash: str, category: str)<br>
<hr>
delete_event_category(hash: str, category: str)<br>
<hr>
edit_event_description(hash: str, description: str)<br>
<hr>
edit_event_location(hash: str, location: str)<br>
<hr>
edit_event_date(hash: str, date_from: int, date_to: int)<br>
<hr>
get_all_events()<br>
response: list[dict]<br>
example:<br>
[<br>
&nbsp;&nbsp;&nbsp;&nbsp; {<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "hash": "3TXKY5",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "owner_id": 1,<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "members": ["user1", "user2"],<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "name": "name",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "categories": ["IT"],<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "description": "desc",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "location": "loc",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "date_from": "date_f",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "date_to": "date_t",<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "public": 1,<br>
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; "online": 1<br>
&nbsp;&nbsp;&nbsp;&nbsp; },<br>
]<br>
<hr>
def add_event_member(event_hash: str, member: str)
<hr>"""

@app.route("/")
def main_handler():
    return "/users<br>/events"

if __name__ == '__main__':
    # app.run(debug=True)
    app.run(debug=True, host='0.0.0.0', port=5000)