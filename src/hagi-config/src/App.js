import "./styles/App.scss";
import { api } from "./api/api";
import React from "react";
import InitialSetup from "./components/initialSetup";
import CreateAccount from "./components/createAccountDialog";
import LoginDialog from "./components/loginDialog";
import Config from "./components/config";
import { ConfigContext, ConfigProvider } from "./configContext";


class App extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      loaded: false,
      isLoggedIn: false,
      initialSetup: false
    };

    this.getStatus = this.getStatus.bind(this);
  }

  loggedIn(isLoggedIn) {
    this.setState({isLoggedIn});
  }

  getStatus() {
    return api.start().then(r => {
      this.setState({
        loaded: true,
        initialSetup: !r.initialSetupComplete,
        accountCreated: r.accountCreated,
        isLoggedIn: api.isLoggedIn()
      });
    });
  }

  componentDidMount() {
    this.getStatus();
  }

  getContent() {
    if (!this.state.loaded) {
      return (
        <h1>Loading</h1>
      );
    } else if (!this.state.accountCreated) {
      return (<CreateAccount onClose={this.getStatus} />);
    } else if (!this.state.isLoggedIn) {
      return (<LoginDialog onClose={this.getStatus} />);
      // } else if (this.state.initialSetup) {
      //     return (<InitialSetup/>);
    } else {
      return (<ConfigProvider><Config/></ConfigProvider>);
    }
  }

  render() {
    return (this.getContent());
  }
}

export default App;
