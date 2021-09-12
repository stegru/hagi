import React from "react";
import { ConfigContext } from "../configContext";

class Config extends React.Component {

  static contextType = ConfigContext;

  constructor(props) {
    super(props);
    this.state = {
      loaded: false
    };

  }

  render() {
    if (!this.configContext) {
      //return (<b>Loading config...</b>);
    }

    return (
      <>
        <pre>{JSON.stringify(this.context, null, "  ")}</pre>
      </>
    );
  }
}


export default Config;
