import React, { useState } from 'react';
import {
  Box,
  TextField,
  Button,
} from '@mui/material';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as Yup from 'yup';

import AlertDialog from '../components/AlertDialog';
import { useNotifier } from '../components/Notifier';

// 1. Define a Yup schema:
const schema = Yup.object().shape({
  cardNumber: Yup.string()
    .required('Card number is required')
    .matches(/^\d{16}$/, 'Must be exactly 16 digits'),
  cvv: Yup.string()
    .required('CVV is required')
    .matches(/^\d{3}$/, 'Must be exactly 3 digits'),
  name: Yup.string().required('Name on card is required'),
});

export default function PaymentForm() {
  const { control, handleSubmit, formState: { errors } } = useForm({
    resolver: yupResolver(schema),
  });
  const { notify } = useNotifier();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [formData, setFormData] = useState(null);

  // 2. On valid submit, show confirmation dialog
  const onSubmit = (data) => {
    setFormData(data);
    setConfirmOpen(true);
  };

  // 3. If user confirms, send & notify
  const handleConfirm = () => {
    setConfirmOpen(false);
    // …call your API with formData…
    notify('Payment submitted successfully!', 'success');
  };

  return (
    <Box
      component="form"
      noValidate
      onSubmit={handleSubmit(onSubmit)}
      sx={{ maxWidth: 400, mx: 'auto', mt: 4 }}
    >
      <Controller
        name="cardNumber"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            label="Card Number"
            fullWidth
            margin="normal"
            error={!!errors.cardNumber}
            helperText={errors.cardNumber?.message}
          />
        )}
      />
      <Controller
        name="cvv"
        control={control}        
        render={({ field }) => (
          <TextField
            {...field}
            label="CVV"
            fullWidth
            margin="normal"
            error={!!errors.cvv}
            helperText={errors.cvv?.message}
          />
        )}
      />
      <Controller
        name="name"
        control={control}
        render={({ field }) => (
          <TextField
            {...field}
            label="Name on Card"
            fullWidth
            margin="normal"
            error={!!errors.name}
            helperText={errors.name?.message}
          />
        )}
      />
      
      <Button
        type="submit"
        fullWidth
        variant="contained"
        sx={{ mt: 2 }}
      >
        Pay Now
      </Button>

      <AlertDialog
        open={confirmOpen}
        title="Confirm Payment"
        message={`Charge ₹${formData?.amount || '0.00'} to card ending ${formData?.cardNumber.slice(-4)}?`}
        onConfirm={handleConfirm}
        onClose={() => setConfirmOpen(false)}
        confirmText="Yes, Charge"
        cancelText="Cancel"
        showCancel
      />
    </Box>
  );
}