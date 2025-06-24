import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Typography
} from '@mui/material';

export default function AlertDialog({
  open,
  title = 'Attention',
  message,
  onConfirm,
  onClose,
  confirmText = 'OK',
  cancelText = 'Cancel',
  showCancel = false,
}) {
  return (
    <Dialog open={open} onClose={onClose}>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <Typography>{message}</Typography>
      </DialogContent>
      <DialogActions>
        {showCancel && (
          <Button onClick={onClose}>
            {cancelText}
          </Button>
        )}
        <Button variant="contained" onClick={onConfirm}>
          {confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  );
}