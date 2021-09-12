import React from "react";
import { Button, Form } from "react-bootstrap";
import InputField from "./inputField";
import Dialog from "./dialog";
import { api } from "../api/api";
import PropTypes from "prop-types";


class CreateAccountDialog extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      username: "",
      password: "",
      passwordConfirm: "",
      valid: undefined
    };

    this.formRef = React.createRef();
    this.passwordConfirmRef = React.createRef();
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
      await api.setPassword(null, null, this.state.username, this.state.password);
      await api.login(this.state.username, this.state.password);
      this.props.onClose && this.props.onClose(true);
    }
  }

  render() {
    return (
      <Dialog show={true} title="HAGI Initial setup" footer={
        <Button onClick={this.submit}>OK</Button>
      }>
        Provide some login details.

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
          <InputField required
                      invalidText="Passwords must match"
                      pattern={this.state.password}
                      label="Password (again)"
                      type="text"
                      name="passwordConfirm"
                      value={this.state.passwordConfirm}
                      onChange={InputField.handleChange(this)} />
        </Form>
      </Dialog>
    );
  }
}

CreateAccountDialog.propTypes = {
  onClose: PropTypes.func
};

export default CreateAccountDialog;
