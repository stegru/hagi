import React from "react";
import PropTypes from "prop-types";
import { Modal } from "react-bootstrap";


class Dialog extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      show: this.props.show
    };

    this.hide = this.hide.bind(this);
  }

  hide() {
    this.setState({show: false});
  }

  render() {
    return (
      <Modal show={this.state.show}>
        <Modal.Header>
          <Modal.Title>{this.props.title}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          {this.props.children}
        </Modal.Body>
        <Modal.Footer>
          {this.props.footer}
        </Modal.Footer>
      </Modal>
    );
  }
}

Dialog.propTypes = {
  show: PropTypes.bool,
  title: PropTypes.string,
  children: PropTypes.any,
  footer: PropTypes.any
};

export default Dialog;
