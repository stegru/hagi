import { Component, createContext, useContext, useState } from "react";
import PropTypes from "prop-types";
import { api } from "./api/api";

export const ConfigContext = createContext({});

/**
 * @typedef {object} ConfigContext
 * @property {object} config
 * @property {Function} updateConfig
 * @property {Function} initialConfig
 */

export class ConfigProvider extends Component {

  constructor(props) {
    super(props);
    this.state = { config: {} };
  }

  componentDidMount() {
    this.loadConfig();
  }

  async loadConfig() {
    const config = await api.getConfig();
    this.setState({
      loaded: true,
      config: config
    });
  }

  updateConfig = (c) => {
    this.setState({config: c});
  };

  render() {
    return (
      <ConfigContext.Provider
            value={{config: this.state.config, updateConfig: this.updateConfig}}>
        {this.props.children}
      </ConfigContext.Provider>
    );
  }
}

ConfigProvider.propTypes = {
  children: PropTypes.any
};


