from flask import Blueprint
from flask import Flask, request, jsonify
from db import *

bp2 = Blueprint('events', __name__,)

@bp2.route("/events/get_event")
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

    return jsonify({"ok": True, "response": ans}), 200


@bp2.route("/events/add_event")
def add_event_handler():
    args = [
        request.args.get('name'),
        request.args.get('owner'),
        request.args.get('categories'),
        request.args.get('description'),
        request.args.get('location'),
        request.args.get('date_from'),
        request.args.get('date_to'),
        request.args.get('public'),
        request.args.get('online')
    ]

    i = 0
    for arg in args:
        
        if not arg:
            return jsonify({"ok": False, "response": f"One of parameters is missing: {i}"})
        
        i += 1
        
    new_event_hash = add_event(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8])
    
    for category in args[1].split(","):
        if category in ["IT", "Social", "Business", "Health", "Creation", "Sport", "Education", "Science"]:
            add_event_category(new_event_hash, category)

    return jsonify({"ok": True, "response": new_event_hash}), 200


@bp2.route("/events/edit_event_name")
def edit_event_name_handler():
    hash = request.args.get('hash')
    name = request.args.get('name')

    if not hash or not name:
        return jsonify({"ok": False, "response": "parameter hash or name is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})
        
    edit_event_name(hash, name)

    return jsonify({"ok": True}), 200


@bp2.route("/events/add_event_category")
def add_event_category_handler():
    hash = request.args.get('hash')
    category = request.args.get('category')

    if not hash or not category:
        return jsonify({"ok": False, "response": "parameter hash or category is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})

    add_event_category(hash, category)

    return jsonify({"ok": True}), 200


@bp2.route("/events/delete_event_category")
def delete_event_category_handler():
    hash = request.args.get('hash')
    category = request.args.get('category')

    if not hash or not category:
        return jsonify({"ok": False, "response": "parameter hash or category is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})

    delete_event_category(hash, category)

    return jsonify({"ok": True}), 200


@bp2.route("/events/edit_event_description")
def edit_event_description_handler():
    hash = request.args.get('hash')
    description = request.args.get('description')

    if not hash or not description:
        return jsonify({"ok": False, "response": "parameter hash or description is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})

    edit_event_description(hash, description)

    return jsonify({"ok": True}), 200


@bp2.route("/events/edit_event_location")
def edit_event_location_handler():
    hash = request.args.get('hash')
    location = request.args.get('location')

    if not hash or not location:
        return jsonify({"ok": False, "response": "parameter hash or location is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})

    edit_event_location(hash, location)

    return jsonify({"ok": True}), 200


@bp2.route("/events/edit_event_date")
def edit_event_date_handler():
    hash = request.args.get('hash')
    date_from = request.args.get('date_from')
    date_to = request.args.get('date_to')

    if not hash or not date_from or not date_to:
        return jsonify({"ok": False, "response": "one of the parameters is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})

    edit_event_location(hash, date_from, date_to)

    return jsonify({"ok": True}), 200


@bp2.route("/events/add_event_member")
def add_event_member_handler():
    username = request.args.get('username')
    hash = request.args.get('hash')

    if not hash or not username:
        return jsonify({"ok": False, "response": "one of the parameters is missing."})
        
    if not find_event(hash):
        return jsonify({"ok": False, "response": "event with current name not found."})
    
    if not find_profile(username):
        return jsonify({"ok": False, "response": "username not found."})

    add_event_member(hash, username)

    return jsonify({"ok": True}), 200


@bp2.route("/events/delete_event")
def delete_event_handler():
    hash = request.args.get('hash')

    if not hash:
        return jsonify({"ok": False, "response": "parameter hash is missing."})

    delete_event(hash)

    return jsonify({"ok": True}), 200


@bp2.route("/events/get_all_events")
def get_all_events_handler():
    return jsonify({"ok": True, "response": get_all_events()}), 200


