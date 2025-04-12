from flask import Flask, request, jsonify
from db import is_user_and_password_correct


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
