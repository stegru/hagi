import React from "react";
import { Form } from "react-bootstrap";
import PropTypes from "prop-types";

class InputField extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
    };
  }

  render() {
    return (
      <Form.Group className="mt-3">
        <Form.Label>{this.props.label}</Form.Label>
        <Form.Control required={this.props.required}
                      type={this.props.type}
                      name={this.props.name}
                      value={this.props.value}
                      pattern={this.props.pattern}
                      onChange={this.props.onChange} />
        <Form.Control.Feedback type="invalid">{this.props.invalidText || "Required"}</Form.Control.Feedback>
      </Form.Group>
    );
  }
}

/**
 * Called by a parent component to provide a change event handler.
 * @param {React.Component} component The component.
 * @returns {Function} The event handler.
 */
InputField.handleChange = function (component) {
  /**
   * Called by the component to handle the change event.
   * @param {Event} event The change data.
   */
  return function (event) {
    component.setState({
      [event.target.name]: event.target.type === "checkbox" ? event.target.checked : event.target.value
    });
  };
};

InputField.propTypes = {
  label: PropTypes.string.isRequired,
  type: PropTypes.oneOf(["text", "email", "password", "checkbox"]).isRequired,
  name: PropTypes.string,
  value: PropTypes.string,
  required: PropTypes.bool,
  pattern: PropTypes.string,
  onChange: PropTypes.func,
  invalidText: PropTypes.string
};

export default InputField;
