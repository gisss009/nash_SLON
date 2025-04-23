from flask import Blueprint, request, jsonify
from db import *

bp3 = Blueprint('likes', __name__,)

# @bp3.route("/likes/create_notification")
# def create_notification_handler():
#     username = request.args.get('username')
#     username_from = request.args.get('username_from')

#     if not username or not username_from:
#         return jsonify({"ok": False, "response": "parameter username or username_from is missing."})

#     create_notification(username, username_from)

#     return jsonify({"ok": True}), 200


# @bp3.route("/likes/delete_notification")
# def delete_notification_handler():
#     username = request.args.get('username')
#     username_from = request.args.get('username_from')

#     if not username or not username_from:
#         return jsonify({"ok": False, "response": "parameter username or username_from is missing."})

#     delete_notification(username, username_from)

#     return jsonify({"ok": True}), 200


@bp3.route("/likes/get_requests")
def get_requests_handler():
    username = request.args.get('username')

    if not username:
        return jsonify({"ok": False, "response": "parameter username is missing."})

    return jsonify({"ok": True, "response": get_requests(username)}), 200


# @bp3.route("/likes/create_accepted_request")
# def create_accepted_request_handler():
#     username = request.args.get('username')
#     username_from = request.args.get('username_from')

#     if not username or not username_from:
#         return jsonify({"ok": False, "response": "parameter username or username_from is missing."})

#     create_accepted_request(username, username_from)

#     return jsonify({"ok": True}), 200


# @bp3.route("/likes/delete_accepted_request")
# def delete_accepted_request_handler():
#     username = request.args.get('username')
#     username_from = request.args.get('username_from')

#     if not username or not username_from:
#         return jsonify({"ok": False, "response": "parameter username or username_from is missing."})

#     delete_accepted_request(username, username_from)

#     return jsonify({"ok": True}), 200


# @bp3.route("/likes/add_mutual_user")
# def add_mutual_user_handler():
#     username = request.args.get('username')
#     username_from = request.args.get('username_from')

#     if not username or not username_from:
#         return jsonify({"ok": False, "response": "parameter username or username_from is missing."})

#     add_mutual_user(username, username_from)

#     return jsonify({"ok": True}), 200


# @bp3.route("/likes/delete_mutual_user")
# def delete_mutual_user_handler():
#     username_one = request.args.get('username_one')
#     username_two = request.args.get('username_two')

#     if not username_one or not username_two:
#         return jsonify({"ok": False, "response": "parameter username_one or username_two is missing."})

#     delete_mutual_user(username_one, username_two)

#     return jsonify({"ok": True}), 200


@bp3.route("/likes/accept_user")
def accept_user_handler():
    username_one = request.args.get('username_one')
    username_two = request.args.get('username_two')

    if not username_one or not username_two:
        return jsonify({"ok": False, "response": "parameter username_one or username_two is missing."})

    create_accepted_request(username_two, username_one)
    delete_profile_swiped_user(username_two, username_one)
    add_mutual_user(username_one, username_two)

    return jsonify({"ok": True}), 200


@bp3.route("/likes/decline_user")
def decline_user_handler():
    username_one = request.args.get('username_one')
    username_two = request.args.get('username_two')

    if not username_one or not username_two:
        return jsonify({"ok": False, "response": "parameter username_one or username_two is missing."})

    delete_notification(username_one, username_two)
    delete_profile_swiped_user(username_two, username_one)

    return jsonify({"ok": True}), 200