% function to read in MCPP infile template, replace strings a1,s1,sp1
% with inverse iterate values and generate pmc and dmc results
function [F,J]=pmc_F_dmc_J_wv(fitparms,wavelengths,rhoMidpoints,absorbers,scatterers,g,n)
% the following code assumes 1-layer tissue with varying mua and mus only
% g and n are fixed and not optimized
% determine rho bins from midpoints
rho=zeros(size(rhoMidpoints,2)+1,1);
rho(1) = rhoMidpoints(1) - (rhoMidpoints(2) - rhoMidpoints(1))/2;
rho(2:end) = rhoMidpoints(1:end) + rhoMidpoints(1);
% determine if fitting chromophore concentrations 
i=0;
if (length(absorbers.Names)>0)
  absorbers.Concentrations=zeros(1,length(absorbers.Names));
  for i=1:length(absorbers.Names)
    absorbers.Concentrations(1,i)=fitparms(i);
  end
end
% and/or scattering coefficients
if (length(scatterers.Names)>0)
  scatterers.Concentrations=zeros(1,length(scatterers.Names));
  for j=1:length(scatterers.Names)
    scatterers.Concentrations(1,i)=fitparms(j+i);
  end
end
[ops,dmua,dmusp] = get_optical_properties(absorbers, scatterers, wavelengths);
F=zeros(length(wavelengths),1);
J=zeros(length(wavelengths),length(fitparms));
% replace MCPP infile with updated OPs
infile_PP='infile_PP_pMC_est.txt';
for iwv=1:length(wavelengths)
  [status]=system(sprintf('cp infile_PP_pMC_est_template.txt %s',infile_PP));
  [status]=system(sprintf('./sub_ops.sh var1 %s %s',sprintf('wv%d',iwv),infile_PP));
  [status]=system(sprintf('./sub_ops.sh a1 %f %s',ops(iwv,1),infile_PP));
  [status]=system(sprintf('./sub_ops.sh s1 %f %s',ops(iwv,2)/(1-g),infile_PP));
  [status]=system(sprintf('./sub_ops.sh sp1 %f %s',ops(iwv,2),infile_PP));
  [status]=system(sprintf('./sub_ops.sh rhostart %f %s',rho(1),infile_PP));
  [status]=system(sprintf('./sub_ops.sh rhostop %f %s',rho(end),infile_PP));
  [status]=system(sprintf('./sub_ops.sh rhocount %d %s',length(rho),infile_PP))
  % run MCPP with updated infile
  [status]=system(sprintf('./mc_post infile=%s',infile_PP));
  [R,pmcR,dmcRmua,dmcRmus]=load_for_inv_results(sprintf('PP_wv%d',iwv));
  F(iwv)=pmcR(4)';
  % set jacobian derivative information 
  if (length(fitparms)==length(absorbers.Names)) % => only chromophore fit
    J(iwv,:) = [dmcRmua(4) * dmua(iwv,:)];
  elseif (length(fitparms)==length(scatterers.Names)) % => only scatterer fit
    J(iwv,:) = [dmcRmus(4) * dmusp(iwv,:)];
  else
    J(iwv,:) = [dmcRmua(4) * dmua(iwv,:) dmcRmus(4) * dmusp(iwv,:)]; % => both
  end
end
end
