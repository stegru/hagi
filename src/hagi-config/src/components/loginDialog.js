import React from "react";
import { Alert, Button, Form } from "react-bootstrap";
import InputField from "./inputField";
import Dialog from "./dialog";
import { api } from "../api/api";
import PropTypes from "prop-types";


class LoginDialog extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      username: "",
      password: "",
      valid: undefined,
      errorMessage: null
    };

    this.formRef = React.createRef();
    this.submit = this.submit.bind(this);
  }

  validate() {
    let valid = this.formRef.current.checkValidity();

    this.setState({
      valid: valid
    });

    return valid;
  }

  async submit() {
    if (this.validate()) {
      try {
        await api.login(this.state.username, this.state.password);
        this.props.onClose && this.props.onClose(true);
      } catch {
        this.setState({errorMessage: "Login failed"});
      }
    }
  }

  render() {
    return (
      <Dialog show={true} title="HAGI Login" footer={
        <>
          <Alert show={!!this.state.errorMessage}
                 variant="danger">
            {this.state.errorMessage}
          </Alert>
          <Button onClick={this.submit}>OK</Button>
        </>
      }>
        <Form noValidate validated={this.state.valid !== undefined} ref={this.formRef}>
          <InputField required
                      label="Username"
                      type="text"
                      name="username"
                      value={this.state.username}
                      onChange={InputField.handleChange(this)} />
          <InputField required
                      label="Password"
                      type="text"
                      name="password"
                      value={this.state.password}
                      onChange={InputField.handleChange(this)} />
        </Form>
      </Dialog>
    );
  }
}

LoginDialog.propTypes = {
  onClose: PropTypes.func
};

export default LoginDialog;
