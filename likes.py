from flask import Blueprint, request, jsonify
from db import *

bp3 = Blueprint('likes', __name__,)

@bp3.route("/likes/get_requests", methods=['GET'])
def get_requests_handler():
    username = request.args.get('username')

    if not username:
        return jsonify({"ok": False, "response": "parameter username is missing."})

    return jsonify({"ok": True, "response": get_requests(username)}), 200

@bp3.route("/likes/get_accepted", methods=['GET'])
def get_accepted_handler():
    username = request.args.get('username')

    if not username:
        return jsonify({"ok": False, "response": "parameter username is missing."})

    return jsonify({"ok": True, "response": get_accepted(username)}), 200


@bp3.route("/likes/accept_user", methods=['POST'])
def accept_user_handler():
    username_one = request.args.get('username_one')
    username_two = request.args.get('username_two')

    if not username_one or not username_two:
        return jsonify({"ok": False, "response": "parameter username_one or username_two is missing."})

    create_accepted_request(username_one, username_two)
    delete_profile_swiped_user(username_two, username_one)
    add_mutual_user(username_one, username_two)
    delete_notification(username_one, username_two)
    
    return jsonify({"ok": True}), 200


@bp3.route("/likes/decline_user", methods=['POST'])
def decline_user_handler():
    username_one = request.args.get('username_one')
    username_two = request.args.get('username_two')

    if not username_one or not username_two:
        return jsonify({"ok": False, "response": "parameter username_one or username_two is missing."})

    delete_notification(username_one, username_two)
    delete_profile_swiped_user(username_two, username_one)

    return jsonify({"ok": True}), 200

@bp3.route("/likes/get_mutual", methods=['GET'])
def get_mutual_handler():
    username = request.args.get('username')
    if not username:
        return jsonify(ok=False, response="parameter username is missing."), 400

    # ���� ������ �� ����, ��� ���� ��� ����
    c.execute("SELECT pair FROM mutual WHERE pair LIKE ? OR pair LIKE ?", (f"{username},%", f"%,{username}"))
    rows = c.fetchall()
    others = []
    for (pair,) in rows:
        u1, u2 = pair.split(',', 1)
        other = u2 if u1 == username else u1
        profile = get_profile(other)
        if profile:
            others.append(profile)

    return jsonify(ok=True, response=others), 200

@bp3.route("/likes/remove_mutual", methods=['POST'])
def remove_mutual_handler():
    username = request.args.get('username')
    other = request.args.get('username_to')
    if not username or not other:
        return jsonify(ok=False, response="parameters missing."), 400

    delete_mutual_user(username, other)
    return jsonify(ok=True), 200
