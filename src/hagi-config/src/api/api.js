import { config } from "../config";
import axios from "axios";


const apiService = axios.create({
  baseURL: config.apiUrl
});

apiService.interceptors.request.use(req => {
  const token = localStorage.getItem("token");
  if (token) {
    req.headers["X-AuthToken"] = token;
  }
  return req;
});

apiService.interceptors.response.use(undefined, function (err) {
  if (err.response?.status === 401 && !err.config?.isLogin) {
    localStorage.removeItem("token");
    location.reload();
  }
});

export const api = {
  isLoggedIn() {
    return !!localStorage.getItem("token");
  },

  start() {
    return apiService.get("/config/start").then(response => response.data);
  },

  async login(username, password) {
    const login = await apiService.post("/config/login", {
      username, password
    }, {isLogin: true}).then(response => response.data);

    localStorage.setItem("token", login.token);
    return login;
  },

  setPassword(username, password, newUsername, newPassword) {
    return apiService.post("/config/password", {
      username, password,
      newUsername, newPassword
    }).then(response => response.data);
  },

  getConfig() {
    return apiService.get("/config").then(response => JSON.parse(response.data.config));
  }
};
