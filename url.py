from flask import Blueprint
from db import *
from flask import request, jsonify, send_from_directory, current_app
from werkzeug.utils import secure_filename

bp5 = Blueprint('url', __name__,)

@bp5.route("/users/add_url", methods=['POST'])
def add_url_handler():
    username = request.args.get('username')
    urls_param = request.args.get('urls')

    if not username or not urls_param:
        return jsonify(ok=False, response="parameters missing."), 400

    urls = [url.strip() for url in urls_param.split(',') if url.strip()]

    if not urls:
        return jsonify(ok=False, response="no valid URLs provided."), 400

    for url in urls:
        add_profile_url(username, url)

    return jsonify(ok=True), 200

@bp5.route("/users/delete_url", methods=['POST'])
def delete_url_handler():
    username   = request.args.get('username')
    urls_param = request.args.get('urls')

    if not username or not urls_param:
        return jsonify(ok=False, response="parameters missing."), 400

    urls = [u.strip() for u in urls_param.split(',') if u.strip()]
    if not urls:
        return jsonify(ok=False, response="no valid URLs provided."), 400

    for url in urls:
        delete_profile_url(username, url)

    remaining = get_profile_urls(username)
    return jsonify(ok=True, urls=remaining), 200



@bp5.route("/users/set_urls", methods=['POST'])
def set_urls_handler():
    username   = request.args.get('username')
    urls_param = request.args.get('urls')

    if not username or not urls_param:
        return jsonify(ok=False, response="parameters missing."), 400

    if urls_param == "none":
        set_profile_urls(username, [])
        return jsonify(ok=True), 200

    urls = [u.strip() for u in urls_param.split(',') if u.strip()]
    if not urls:
        return jsonify(ok=False, response="no valid URLs provided."), 400

    updated = set_profile_urls(username, urls)
    if not updated:
        return jsonify(ok=False, response="profile not found."), 404

    # Возвращаем новый список ссылок
    return jsonify(ok=True, urls=urls), 200


@bp5.route("/users/get_urls", methods=['POST'])
def get_urls_handler():
    username   = request.args.get('username')

    if not username:
        return jsonify(ok=False, response="parameter missing."), 400

    # Возвращаем новый список ссылок
    return jsonify(ok=True, urls=get_profile_urls(username)), 200
