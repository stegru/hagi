
const configKey = (!process.env.NODE_DEV || process.env.NODE_DEV === "development")
    ? "DEV"
    : "PROD";

/**
 * @typedef {Object} AppConfig
 * @property {String} apiUrl
 */

/**
 * @type {Object<string,AppConfig>}
 */
const configs = {
    BASE: {
        apiUrl: "/hagi/"
    },
    DEV: {
        apiUrl: "http://localhost:5580/hagi/"
    },
    PROD: {
        apiUrl: "/hagi/"
    }
};

/** @type {AppConfig} */
export const config = Object.assign({}, configs.BASE, configs[configKey]);
